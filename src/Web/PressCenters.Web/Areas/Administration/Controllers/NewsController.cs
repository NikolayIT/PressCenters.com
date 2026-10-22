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

        // These actions change data, so they are POST + antiforgery: as GET links any page could trigger them
        // through a logged-in administrator's browser (the auth cookie is sent on cross-site top-level GETs).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRemoteNews(int id)
        {
            var news = this.newsRepository.AllWithDeleted().Where(x => x.Id == id)
                .Select(x => new { x.OriginalUrl, x.Source.TypeName, }).FirstOrDefault();
            if (news?.TypeName == null || news.OriginalUrl == null)
            {
                return this.NotFound();
            }

            var executor = ReflectionHelpers.GetInstance<BaseSource>(news.TypeName);
            var remoteNews = executor.GetPublication(news.OriginalUrl);
            if (remoteNews == null)
            {
                // The original could not be fetched or parsed.
                return this.NotFound();
            }

            await this.newsService.UpdateAsync(id, remoteNews);
            await this.newsService.SaveImageAsync(remoteNews.ImageUrl, id, true, executor.CheckCertificateRevocation);

            return this.RedirectToAction("ById", "News", new { area = string.Empty, id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var news = this.newsRepository.All().FirstOrDefault(x => x.Id == id);
            if (news == null)
            {
                return this.NotFound();
            }

            this.newsRepository.Delete(news);
            await this.newsRepository.SaveChangesAsync();

            return this.RedirectToAction("List", "News", new { area = string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var news = this.newsRepository.All().FirstOrDefault(x => x.Id == id);
            if (news == null)
            {
                return this.NotFound();
            }

            this.newsRepository.HardDelete(news);
            await this.newsRepository.SaveChangesAsync();

            return this.RedirectToAction("List", "News", new { area = string.Empty });
        }
    }
}
