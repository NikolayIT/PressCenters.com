namespace PressCenters.Worker.Runner
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using PressCenters.Worker.Common;
    using PressCenters.Worker.Tasks;

    public class TaskRunnerHostedService : IHostedService
    {
        private readonly ICollection<ITaskExecutor> taskExecutors = new List<ITaskExecutor>();
        private readonly IList<Thread> threads = new List<Thread>();
        private readonly ConcurrentDictionary<int, bool> tasksIds;
        private readonly IServiceCollection serviceCollection;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;

        public TaskRunnerHostedService(
            IServiceCollection serviceCollection,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            var threadsCount = int.Parse(configuration["JobScheduler:ThreadsCount"]);
            this.tasksIds = new ConcurrentDictionary<int, bool>(threadsCount, 1024);
            for (var i = 1; i <= threadsCount; i++)
            {
                var thread = new Thread(this.CreateAndStartTaskExecutor) { Name = $"Thread #{i}" };
                this.threads.Add(thread);
            }

            this.serviceCollection = serviceCollection;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<TaskRunnerHostedService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            for (var i = 0; i < this.threads.Count; i++)
            {
                var thread = this.threads[i];

                this.logger.LogInformation($"Starting {thread.Name}...");
                thread.Start(i + 1);
                this.logger.LogInformation($"{thread.Name} started.");

                // Give the thread some time to start properly before starting another threads.
                // (Prevents "System.ArgumentException: An item with the same key has already been added" related to the DI framework)
                Thread.Sleep(400);
            }

            this.logger.LogInformation($"{nameof(TaskRunnerHostedService)} started with {this.threads.Count} threads.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var taskExecutor in this.taskExecutors)
            {
                taskExecutor.Stop();
            }

            this.logger.LogInformation($"{nameof(TaskRunnerHostedService)} stopped.");
            return Task.CompletedTask;
        }

        private async void CreateAndStartTaskExecutor(object taskExecutorNumber)
        {
            var taskExecutorName = $"{nameof(TaskExecutor)} #{taskExecutorNumber}";
            ITaskExecutor taskExecutor = null;

            // This is an important exception handling because an exception in async void method would crash the app.
            // (https://blogs.msdn.microsoft.com/ptorr/2014/12/10/async-exceptions-in-c/)
            try
            {
                taskExecutor = new TaskExecutor(
                    taskExecutorName,
                    this.tasksIds,
                    this.serviceCollection,
                    this.loggerFactory,
                    typeof(DbCleanupTask).Assembly);

                this.taskExecutors.Add(taskExecutor);

                await taskExecutor.Work();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical(
                    taskExecutor == null
                        ? $"Error during initialization of {taskExecutorName}: {ex}"
                        : $"Error during {taskExecutorName}'s work: {ex}");
            }
        }
    }
}
