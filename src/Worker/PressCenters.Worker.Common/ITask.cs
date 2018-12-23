namespace PressCenters.Worker.Common
{
    using System.Threading.Tasks;

    using PressCenters.Data.Models;

    public interface ITask
    {
        Task<string> DoWork(string parameters);

        WorkerTask Recreate(WorkerTask currentTask);
    }
}
