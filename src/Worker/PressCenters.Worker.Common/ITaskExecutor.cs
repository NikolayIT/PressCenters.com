namespace PressCenters.Worker.Common
{
    using System.Threading.Tasks;

    public interface ITaskExecutor
    {
        Task Work();

        void Stop();
    }
}
