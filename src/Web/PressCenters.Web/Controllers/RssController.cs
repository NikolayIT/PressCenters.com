namespace PressCenters.Web.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services;

    [AllowAnonymous]
    public class RssController : BaseController
    {
        private const int NewsCount = 20;

        private readonly IDeletableEntityRepository<News> newsRepository;

        private readonly ISlugGenerator slugGenerator;

        public RssController(IDeletableEntityRepository<News> newsRepository, ISlugGenerator slugGenerator)
        {
            this.newsRepository = newsRepository;
            this.slugGenerator = slugGenerator;
        }

        public async Task<IActionResult> Latest(int? id)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            sb.AppendLine("<rss version=\"2.0\">");
            sb.AppendLine("<channel>");
            sb.AppendLine($"<title>{GlobalConstants.SystemName}</title>");
            sb.AppendLine($"<link>{GlobalConstants.SystemBaseUrl}</link>");
            sb.AppendLine($"<description>{GlobalConstants.SystemSlogan}</description>");

            var query = this.newsRepository.AllAsNoTracking();
            if (id.HasValue)
            {
                query = query.Where(x => x.SourceId == id);
            }

            var news = await query.OrderByDescending(x => x.CreatedOn).Take(NewsCount)
                           .Select(x => new { x.Id, x.Title, x.CreatedOn, SourceName = x.Source.Name }).ToListAsync();

            foreach (var newsItem in news)
            {
                var url = $"{GlobalConstants.SystemBaseUrl}/News/{newsItem.Id}/{this.slugGenerator.GenerateSlug(newsItem.Title)}";
                sb.AppendLine($"<item><title>{WebUtility.HtmlEncode(newsItem.Title)}</title><link>{url}</link><pubDate>{newsItem.CreatedOn:R}</pubDate><category>{newsItem.SourceName}</category></item>");
            }

            sb.AppendLine("</channel>");
            sb.AppendLine("</rss>");
            return this.Content(sb.ToString(), "application/xml");
        }
    }
}
