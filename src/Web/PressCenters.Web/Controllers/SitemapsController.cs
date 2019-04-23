namespace PressCenters.Web.Controllers
{
    using System.Linq;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services;

    [AllowAnonymous]
    public class SitemapsController : BaseController
    {
        private const string BaseUrl = "https://presscenters.com";
        private const int UrlsPerFile = 50000;

        private readonly IDeletableEntityRepository<News> newsRepository;

        private readonly ISlugGenerator slugGenerator;

        public SitemapsController(IDeletableEntityRepository<News> newsRepository, ISlugGenerator slugGenerator)
        {
            this.newsRepository = newsRepository;
            this.slugGenerator = slugGenerator;
        }

        public async Task<IActionResult> Sitemap(int id)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            var news = await this.newsRepository.AllAsNoTracking().OrderBy(x => x.Id).Skip((id - 1) * UrlsPerFile)
                .Take(UrlsPerFile).Select(x => new { x.Id, x.Title, x.CreatedOn, }).ToListAsync();

            foreach (var newsItem in news)
            {
                var url = $"{BaseUrl}/News/{newsItem.Id}/{this.slugGenerator.GenerateSlug(newsItem.Title)}";
                sb.AppendLine($"<url><loc>{url}</loc><lastmod>{newsItem.CreatedOn:yyyy-MM-dd}</lastmod></url>");
            }

            sb.AppendLine("</urlset>");
            return this.Content(sb.ToString(), "application/xml");
        }
    }
}
