namespace PressCenters.Services.CronJobs
{
    using System.Threading.Tasks;

    using PressCenters.Data.Common;
    using PressCenters.Data.Models;

    public class DbCleanupJob
    {
        private readonly IDbQueryRunner queryRunner;

        public DbCleanupJob(IDbQueryRunner queryRunner)
        {
            this.queryRunner = queryRunner;
        }

        public async Task Work()
        {
            await this.queryRunner.RunQueryAsync(
                $"ALTER INDEX [PK_{nameof(News)}] ON [dbo].[{nameof(News)}] REBUILD;");

            await this.queryRunner.RunQueryAsync(
                $"ALTER INDEX [PK_{nameof(MainNews)}] ON [dbo].[{nameof(MainNews)}] REBUILD;");
        }
    }
}
