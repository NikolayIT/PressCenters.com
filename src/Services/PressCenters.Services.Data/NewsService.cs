namespace PressCenters.Services.Data
{
    using System.Linq;
    using System.Threading.Tasks;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;

    public class NewsService : INewsService
    {
        private readonly IDeletableEntityRepository<News> newsRepository;

        public NewsService(IDeletableEntityRepository<News> newsRepository)
        {
            this.newsRepository = newsRepository;
        }

        public async Task<bool> AddAsync(RemoteNews remoteNews, int sourceId)
        {
            if (this.newsRepository.AllWithDeleted().Any(x => x.SourceId == sourceId && x.RemoteId == remoteNews.RemoteId))
            {
                // Already exists
                return false;
            }

            var dbNews = new News
                         {
                             Title = remoteNews.Title?.Trim(),
                             OriginalUrl = remoteNews.OriginalUrl?.Trim(),
                             ImageUrl = remoteNews.ImageUrl?.Trim(),
                             Content = remoteNews.Content?.Trim(),
                             CreatedOn = remoteNews.PostDate,
                             SourceId = sourceId,
                             RemoteId = remoteNews.RemoteId?.Trim(),
                         };

            await this.newsRepository.AddAsync(dbNews);
            await this.newsRepository.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAsync(int id, RemoteNews remoteNews)
        {
            var news = this.newsRepository.AllWithDeleted().FirstOrDefault(x => x.Id == id);
            if (news == null)
            {
                return;
            }

            news.Title = remoteNews.Title;
            news.OriginalUrl = remoteNews.OriginalUrl;
            news.ImageUrl = remoteNews.ImageUrl;
            news.Content = remoteNews.Content;
            news.RemoteId = remoteNews.RemoteId;
            //// We should not update the PostDate here

            await this.newsRepository.SaveChangesAsync();
        }
    }
}
