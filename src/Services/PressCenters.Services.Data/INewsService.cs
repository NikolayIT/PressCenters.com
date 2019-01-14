namespace PressCenters.Services.Data
{
    using System.Threading.Tasks;

    using PressCenters.Data.Models;

    public interface INewsService
    {
        Task<bool> AddAsync(RemoteNews remoteNews, int sourceId);

        Task UpdateAsync(int id, RemoteNews remoteNews);

        int Count();

        string GetSearchText(News news);
    }
}
