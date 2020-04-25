namespace PressCenters.Services.Data
{
    using System.Threading.Tasks;

    using PressCenters.Data.Models;

    public interface INewsService
    {
        Task<int?> AddAsync(RemoteNews remoteNews, int sourceId);

        Task UpdateAsync(int id, RemoteNews remoteNews);

        int Count();

        string GetSearchText(News news);

        Task<bool> SaveImageLocallyAsync(string imageUrl, int newsId, string webRoot, bool useProxy = false);
    }
}
