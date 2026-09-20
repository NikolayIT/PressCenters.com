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

        /// <summary>
        /// Downloads the article image, generates the big and small thumbnails and stores them in the
        /// configured <see cref="Images.IImageStorage"/> under <see cref="Images.NewsImagePaths"/> keys.
        /// </summary>
        Task<bool> SaveImageAsync(string imageUrl, int newsId, bool useProxy = false);
    }
}
