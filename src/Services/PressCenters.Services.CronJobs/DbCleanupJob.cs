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

            // Hangfire keeps Failed jobs forever (their ExpireAt stays NULL), so permanently-broken external
            // sources (sites returning 403/409/522) drip ~1 failed job every few minutes and the table grows
            // without bound (it had reached ~5M). Purge Failed jobs older than 7 days in small batches; the FK
            // cascade also clears their HangFire.State and HangFire.JobParameter rows. Runs on the app's
            // SqlClient connection (QUOTED_IDENTIFIER ON) -- required for DML on Job's filtered index.
            await this.queryRunner.RunQueryAsync(
                @"SET NOCOUNT ON;
DECLARE @r INT = 1;
WHILE @r > 0
BEGIN
    DELETE TOP (5000) FROM [HangFire].[Job]
    WHERE [StateName] = N'Failed' AND [CreatedAt] < DATEADD(DAY, -7, GETUTCDATE());
    SET @r = @@ROWCOUNT;
END");
        }
    }
}
