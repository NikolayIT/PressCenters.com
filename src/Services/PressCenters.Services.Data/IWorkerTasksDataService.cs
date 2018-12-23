namespace PressCenters.Services.Data
{
    using PressCenters.Data.Models;

    public interface IWorkerTasksDataService
    {
        WorkerTask GetForProcessing();

        void Update(WorkerTask workerTask);

        void Add(WorkerTask workerTask);
    }
}
