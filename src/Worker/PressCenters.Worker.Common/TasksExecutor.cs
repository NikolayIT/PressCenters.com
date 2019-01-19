namespace PressCenters.Worker.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using PressCenters.Data.Models;
    using PressCenters.Services.Data;

    public class TasksExecutor : IHostedService
    {
        private const int WaitTimeOnErrorInSeconds = 20;
        private static readonly ConcurrentDictionary<int, bool> TasksIds = new ConcurrentDictionary<int, bool>(4, 1024);
        private static int nextId;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;
        private readonly Assembly tasksAssembly;
        private bool stopping;

        public TasksExecutor(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            ITasksAssemblyProvider assemblyProvider)
        {
            this.serviceProvider = serviceProvider;
            var id = Interlocked.Increment(ref nextId);
            this.logger = loggerFactory.CreateLogger($"{nameof(TasksExecutor)} #{id}");
            this.tasksAssembly = assemblyProvider.GetAssembly();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
#pragma warning disable 4014
            this.RunContinuouslyAsync(cancellationToken);
#pragma warning restore 4014
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.stopping = true;
            this.logger.LogInformation("Stopping...");

            // ReSharper disable once MethodSupportsCancellation
            await Task.Delay(3000);
        }

        private async Task RunContinuouslyAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting...");
            while (!this.stopping && !cancellationToken.IsCancellationRequested)
            {
                // New scope is created for each task that's being executed
                using (var serviceScope = this.serviceProvider.CreateScope())
                {
                    await this.ExecuteNextTask(serviceScope.ServiceProvider);
                    await Task.Delay(1000, cancellationToken);
                }
            }

            this.logger.LogInformation("Stopped.");
        }

        private async Task ExecuteNextTask(IServiceProvider scopedServiceProvider)
        {
            var workerTasksData = scopedServiceProvider.GetService<IWorkerTasksDataService>();
            WorkerTask workerTask;
            try
            {
                workerTask = workerTasksData.GetForProcessing();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Unable to get task for processing. Error: {ex}");
                await Task.Delay(WaitTimeOnErrorInSeconds * 1000);
                return;
            }

            if (workerTask == null)
            {
                // No task available.
                return;
            }

            if (!TasksIds.TryAdd(workerTask.Id, true))
            {
                // Other thread is processing the same task.
                return;
            }

            try
            {
                workerTask.Processing = true;
                await workerTasksData.UpdateAsync(workerTask);
            }
            catch (Exception ex)
            {
                TasksIds.TryRemove(workerTask.Id, out _);
                this.logger.LogError($"Unable to set workerTask.{nameof(WorkerTask.Processing)} to true! Error: {ex}");
                await Task.Delay(WaitTimeOnErrorInSeconds * 1000);
                return;
            }

            this.logger.LogInformation($"Task #{workerTask.Id} started...");

            ITask task = null;
            try
            {
                task = this.GetTaskInstance(workerTask.TypeName, scopedServiceProvider);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Exception in {nameof(this.GetTaskInstance)} on task #{workerTask.Id}: {ex}");
                workerTask.ProcessingComment = $"Error in {nameof(this.GetTaskInstance)}: {ex}";
            }

            if (task == null)
            {
                try
                {
                    workerTask.Processed = true;
                    workerTask.Processing = false;
                    await workerTasksData.UpdateAsync(workerTask);
                    TasksIds.TryRemove(workerTask.Id, out _);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Unable to save final changes on task #{workerTask.Id}! Error: {ex}");
                    await Task.Delay(WaitTimeOnErrorInSeconds * 1000);
                }

                return;
            }

            // Call DoWork()
            string result = null;
            var doWorkStopwatch = Stopwatch.StartNew();
            try
            {
                result = await task.DoWork(workerTask.Parameters);
                doWorkStopwatch.Stop();
                this.logger.LogInformation(
                    $"Task #{workerTask.Id} completed in {doWorkStopwatch.Elapsed} ({DateTime.UtcNow}) with result: {result}");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error in {nameof(ITask.DoWork)} on task #{workerTask.Id}: {ex}");
                workerTask.ProcessingComment = $"Error in {nameof(ITask.DoWork)}: {ex}";
            }

            // Call Recreate()
            WorkerTask nextTask = null;
            try
            {
                nextTask = task.Recreate(workerTask);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error in {nameof(ITask.Recreate)} on task #{workerTask.Id}: {ex}");
                workerTask.ProcessingComment += $"Error in {nameof(ITask.Recreate)}: {ex}";
            }

            // Save result
            try
            {
                workerTask.Result = result;
                workerTask.Duration = doWorkStopwatch.Elapsed.TotalDays >= 1.0
                                          ? new TimeSpan(0, 23, 59, 59)
                                          : doWorkStopwatch.Elapsed;
                workerTask.Processed = true;
                workerTask.Processing = false;
                await workerTasksData.UpdateAsync(workerTask);
                TasksIds.TryRemove(workerTask.Id, out _);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Unable to save result on task #{workerTask.Id}! Error: {ex}");
                await Task.Delay(WaitTimeOnErrorInSeconds * 1000);
                return;
            }

            // Save the new task
            if (nextTask != null)
            {
                try
                {
                    await workerTasksData.AddAsync(nextTask);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Unable to recreate task #{workerTask.Id}! Error: {ex}");
                    await Task.Delay(WaitTimeOnErrorInSeconds * 1000);
                }
            }
        }

        private ITask GetTaskInstance(string typeName, IServiceProvider serviceProvider)
        {
            var type = this.tasksAssembly.GetType(typeName);
            if (!(Activator.CreateInstance(type, serviceProvider) is ITask task))
            {
                throw new Exception($"Unable to create {nameof(ITask)} instance from \"{typeName}\"!");
            }

            return task;
        }
    }
}
