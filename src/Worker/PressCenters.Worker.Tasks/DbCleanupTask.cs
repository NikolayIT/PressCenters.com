namespace PressCenters.Worker.Tasks
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using PressCenters.Data.Common;
    using PressCenters.Data.Models;
    using PressCenters.Worker.Common;

    public class DbCleanupTask : BaseTask<DbCleanupTask.Input, DbCleanupTask.Output>
    {
        private readonly IDbQueryRunner queryRunner;

        private readonly ILogger<DbCleanupTask> logger;

        public DbCleanupTask(IDbQueryRunner queryRunner, ILoggerFactory loggerFactory)
        {
            this.queryRunner = queryRunner;
            this.logger = loggerFactory.CreateLogger<DbCleanupTask>();
        }

        protected override async Task<Output> DoWork(Input input)
        {
            await this.queryRunner.RunQueryAsync(
                $"ALTER INDEX [PK_{nameof(News)}] ON [dbo].[{nameof(News)}] REBUILD;");
            this.logger.LogInformation($"Index [PK_{nameof(News)}] rebuilt.");

            await this.queryRunner.RunQueryAsync(
                $"ALTER INDEX [PK_{nameof(MainNews)}] ON [dbo].[{nameof(MainNews)}] REBUILD;");
            this.logger.LogInformation($"Index [PK_{nameof(MainNews)}] rebuilt.");

            return new Output();
        }

        protected override WorkerTask Recreate(WorkerTask currentTask, Input currentParameters, Output currentResult) =>
            new WorkerTask(currentTask, DateTime.UtcNow.AddDays(7).Date.AddHours(4));

        public class Input : BaseTaskInput
        {
        }

        public class Output : BaseTaskOutput
        {
        }
    }
}
