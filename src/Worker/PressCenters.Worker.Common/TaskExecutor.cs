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

            this.logger.LogInformation($"Started work with task #{workerTask.Id}");

            ITask task = null;
            try
            {
                task = this.GetTaskInstance(workerTask.TypeName, serviceProvider);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"{nameof(this.GetTaskInstance)} on task #{workerTask.Id} has thrown an exception: {ex}");
                workerTask.ProcessingComment = $"Error in {nameof(this.GetTaskInstance)}: {ex}";
            }

            if (task != null)
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();

                    workerTask.Result = await task.DoWork(workerTask.Parameters);

                    workerTask.Duration = stopwatch.Elapsed.TotalDays >= 1.0
                                              ? new TimeSpan(0, 23, 59, 59)
                                              : stopwatch.Elapsed;

                    this.logger.LogInformation(
                        $"Task #{workerTask.Id} completed in {stopwatch.Elapsed} ({DateTime.UtcNow}) with result: {workerTask.Result}");

                    await workerTasksData.UpdateAsync(workerTask);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        $"Task #{workerTask.Id} has thrown an exception: {ex}");

                    workerTask.ProcessingComment = $"Error in {nameof(ITask.DoWork)}: {ex}";
                }

                try
                {
                    var nextTask = task.Recreate(workerTask);
                    if (nextTask != null)
                    {
                        await workerTasksData.AddAsync(nextTask);
                    }

                    await workerTasksData.UpdateAsync(workerTask);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        $"{nameof(ITask.Recreate)} on task #{workerTask.Id} has thrown an exception: {ex}");

                    workerTask.ProcessingComment = $"Error in {nameof(ITask.Recreate)}: {ex}";
                }
            }

            try
            {
                workerTask.Processed = true;
                workerTask.Processing = false;
                await workerTasksData.UpdateAsync(workerTask);

                // For re-runs
                this.tasksIds.TryRemove(workerTask.Id, out _);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Unable to save final changes to the task #{workerTask.Id}! Error: {ex}");

                await Task.Delay(20 * 1000);
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
