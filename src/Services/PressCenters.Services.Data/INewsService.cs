namespace PressCenters.Services.Data
{
    using System.Threading.Tasks;

    using PressCenters.Services.Sources;

    public interface INewsService
    {
        Task<bool> AddAsync(RemoteNews remoteNews, int sourceId);
    }
}
