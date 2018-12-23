namespace PressCenters.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Data.Models;

    public class WorkerTasksSeeder : ISeeder
    {
        public void Seed(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var workerTasks = new List<WorkerTask>
                              {
                                  new WorkerTask
                                  {
                                      TypeName = "PressCenters.Worker.Tasks.DbCleanupTask",
                                      Parameters = "{}",
                                      Priority = 0,
                                  },
                              };

            foreach (var workerTask in workerTasks)
            {
                if (!dbContext.WorkerTasks.Any(x => x.TypeName == workerTask.TypeName))
                {
                    dbContext.WorkerTasks.Add(workerTask);
                }
            }
        }
    }
}
