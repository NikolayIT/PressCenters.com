namespace PressCenters.Worker.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using PressCenters.Data.Models;
    using PressCenters.Services.Data;

    public class TaskExecutor : ITaskExecutor
    {
        private const int WaitTimeOnErrorInSeconds = 20;
        private readonly ConcurrentDictionary<int, bool> tasksIds;
        private readonly IServiceCollection serviceCollection;
        private readonly Assembly tasksAssembly;
        private readonly ILogger logger;
        private bool stopping;

        public TaskExecutor(
            string name,
            ConcurrentDictionary<int, bool> tasksIds,
            IServiceCollection serviceCollection,
            ILoggerFactory loggerFactory,
            Assembly tasksAssembly)
        {
            this.tasksIds = tasksIds;
            this.serviceCollection = serviceCollection;
            this.logger = loggerFactory.CreateLogger(name);
            this.tasksAssembly = tasksAssembly;
        }

        public async Task Work()
        {
            this.logger.LogInformation("Starting...");
            while (!this.stopping)
            {
                // New scope is created for each task that's being executed
                using (var serviceProvider = this.serviceCollection.BuildServiceProvider())
                {
                    await this.ExecuteNextTask(serviceProvider);
                    await Task.Delay(1000);
                }
            }

            this.logger.LogInformation("Stopped.");
        }

        public void Stop()
        {
            this.stopping = true;
        }

        private async Task ExecuteNextTask(IServiceProvider serviceProvider)
        {
            var workerTasksData = serviceProvider.GetService<IWorkerTasksDataService>();
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

            if (!this.tasksIds.TryAdd(workerTask.Id, true))
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
                this.tasksIds.TryRemove(workerTask.Id, out _);
                this.logger.LogError($"Unable to set workerTask.{nameof(WorkerTask.Processing)} to true! Error: {ex}");
                await Task.Delay(WaitTimeOnErrorInSeconds * 1000);
                return;
            }

            this.logger.LogInformation($"Task #{workerTask.Id} started...");

            ITask task = null;
            try
            {
                task = this.GetTaskInstance(workerTask.TypeName, serviceProvider);
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
                    this.tasksIds.TryRemove(workerTask.Id, out _);
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
                this.tasksIds.TryRemove(workerTask.Id, out _);
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
