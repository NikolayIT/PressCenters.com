namespace PressCenters.Services.Data
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;

    public class WorkerTasksDataService : IWorkerTasksDataService
    {
        private readonly IRepository<WorkerTask> workerTasks;

        public WorkerTasksDataService(IRepository<WorkerTask> workerTasks)
        {
            this.workerTasks = workerTasks;
        }

        public WorkerTask GetForProcessing()
            => this.workerTasks
                .All()
                .Where(x => !x.Processed && !x.Processing && (x.RunAfter == null || x.RunAfter <= DateTime.UtcNow))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.Id)
                .FirstOrDefault();

        public void Update(WorkerTask workerTask)
        {
            if (workerTask == null)
            {
                throw new ArgumentNullException(nameof(workerTask), "Worker task to update is required.");
            }

            this.workerTasks.Update(workerTask);
            this.workerTasks.SaveChangesAsync().GetAwaiter().GetResult();
        }

        public async Task Add(WorkerTask workerTask)
        {
            if (workerTask == null)
            {
                throw new ArgumentNullException(nameof(workerTask), "Worker task to add is required.");
            }

            await this.workerTasks.AddAsync(workerTask);
            await this.workerTasks.SaveChangesAsync();
        }
    }
}
