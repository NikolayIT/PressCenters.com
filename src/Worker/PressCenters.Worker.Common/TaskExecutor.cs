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

    public class TaskExecutor
    {
        private const int IdleTimeInSeconds = 1;
        private const int WaitTimeOnErrorInSeconds = 30;

        private readonly string name;

        private readonly ConcurrentDictionary<int, bool> tasksIds;

        private readonly IWorkerTasksDataService workerTasksData;

        private readonly IServiceProvider serviceProvider;

        private readonly Assembly tasksAssembly;

        private readonly ILogger logger;

        private bool stopping;

        public TaskExecutor(
            string name,
            ConcurrentDictionary<int, bool> tasksIds,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            Assembly tasksAssembly)
        {
            this.name = name;
            this.tasksIds = tasksIds;
            this.serviceProvider = serviceProvider;
            this.workerTasksData = serviceProvider.GetService<IWorkerTasksDataService>();
            this.tasksAssembly = tasksAssembly;
            this.logger = loggerFactory.CreateLogger<TaskExecutor>();
        }

        public async Task Work()
        {
            this.logger.LogInformation($"{this.name} starting...");
            while (!this.stopping)
            {
                await this.ExecuteNextTask();
                await Task.Delay(1000);
            }

            this.logger.LogInformation($"{this.name} stopped.");
        }

        public void Stop()
        {
            this.stopping = true;
        }

        private async Task ExecuteNextTask()
        {
            WorkerTask workerTask;
            try
            {
                workerTask = this.workerTasksData.GetForProcessing();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Unable to get task for processing. Error: {ex}");

                await Task.Delay(WaitTimeOnErrorInSeconds * 1000);
                return;
            }

            if (workerTask == null)
            {
                // No task available. Wait few seconds and try again.
                await Task.Delay(IdleTimeInSeconds * 1000);
                return;
            }

            if (!this.tasksIds.TryAdd(workerTask.Id, true))
            {
                // Other thread is processing the same task.
                // Wait the other thread to set Processing to true and then get new from the DB.
                await Task.Delay(100);
                return;
            }

            try
            {
                workerTask.Processing = true;
                await this.workerTasksData.UpdateAsync(workerTask);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Unable to set workerTask.{nameof(WorkerTask.Processing)} to true! Error: {ex}");

                this.tasksIds.TryRemove(workerTask.Id, out _);

                await Task.Delay(WaitTimeOnErrorInSeconds * 1000);
                return;
            }

            this.logger.LogInformation($"{this.name} started work with task #{workerTask.Id}");

            // New scope is created for each task that's being executed
            using (var taskServiceScope = this.serviceProvider.CreateScope())
            {
                ITask task = null;
                try
                {
                    task = this.GetTaskInstance(workerTask.TypeName, taskServiceScope.ServiceProvider);
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

                        await this.workerTasksData.UpdateAsync(workerTask);
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
                            await this.workerTasksData.AddAsync(nextTask);
                        }

                        await this.workerTasksData.UpdateAsync(workerTask);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(
                            $"{nameof(ITask.Recreate)} on task #{workerTask.Id} has thrown an exception: {ex}");

                        workerTask.ProcessingComment = $"Error in {nameof(ITask.Recreate)}: {ex}";
                    }
                }
            }

            try
            {
                workerTask.Processed = true;
                workerTask.Processing = false;
                await this.workerTasksData.UpdateAsync(workerTask);

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

        private ITask GetTaskInstance(string typeName, IServiceProvider scopedServiceProvider)
        {
            var type = this.tasksAssembly.GetType(typeName);
            if (!(Activator.CreateInstance(type, scopedServiceProvider) is ITask task))
            {
                throw new Exception($"Unable to create {nameof(ITask)} variable from \"{typeName}\"!");
            }

            return task;
        }
    }
}
