namespace PressCenters.Services.Data
{
    using System.Threading.Tasks;

    using PressCenters.Data.Models;

    public interface IWorkerTasksDataService
    {
        WorkerTask GetForProcessing();

        void Update(WorkerTask workerTask);

        Task Add(WorkerTask workerTask);
    }
}
