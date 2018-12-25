namespace PressCenters.Services.Data
{
    using System.Threading.Tasks;

    using PressCenters.Services.Sources;

    public interface INewsService
    {
        Task AddAsync(RemoteNews remoteNews, int sourceId);
    }
}
