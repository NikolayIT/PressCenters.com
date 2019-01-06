namespace PressCenters.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Sources;

    public class NewsController : AdministrationController
    {
        private readonly INewsService newsService;

        private readonly IDeletableEntityRepository<News> newsRepository;

        public NewsController(
            INewsService newsService,
            IDeletableEntityRepository<News> newsRepository)
        {
            this.newsService = newsService;
            this.newsRepository = newsRepository;
        }

        public async Task<IActionResult> UpdateRemoteNews(int id)
        {
            var news = this.newsRepository.AllWithDeleted().Where(x => x.Id == id)
                .Select(x => new { x.OriginalUrl, x.Source.TypeName, }).FirstOrDefault();
            if (news == null)
            {
                return this.NotFound();
            }

            var executor = ReflectionHelpers.GetInstance<BaseSource>(news.TypeName);
            var remoteNews = executor.GetPublication(news.OriginalUrl);
            await this.newsService.UpdateAsync(id, remoteNews);

            return this.RedirectToAction("ById", "News", new { area = string.Empty, id });
        }

        public async Task<IActionResult> SoftDelete(int id)
        {
            var news = this.newsRepository.All().FirstOrDefault(x => x.Id == id);
            this.newsRepository.Delete(news);
            await this.newsRepository.SaveChangesAsync();

            return this.RedirectToAction("List", "News", new { area = string.Empty });
        }

        public async Task<IActionResult> HardDelete(int id)
        {
            var news = this.newsRepository.All().FirstOrDefault(x => x.Id == id);
            this.newsRepository.HardDelete(news);
            await this.newsRepository.SaveChangesAsync();

            return this.RedirectToAction("List", "News", new { area = string.Empty });
        }
    }
}
