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
    using PressCenters.Services.Mapping;
    using PressCenters.Web.ViewModels.News;

    [AllowAnonymous]
    public class RssController : BaseController
    {
        private const int NewsCount = 20;

        private readonly IDeletableEntityRepository<News> newsRepository;

        public RssController(IDeletableEntityRepository<News> newsRepository)
        {
            this.newsRepository = newsRepository;
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

            var news = await query.OrderByDescending(x => x.CreatedOn).Take(NewsCount).To<NewsViewModel>().ToListAsync();

            foreach (var newsItem in news)
            {
                sb.AppendLine($@"<item>
    <title>{newsItem.SourceShortName}: {WebUtility.HtmlEncode(newsItem.Title)}</title>
    <link>{GlobalConstants.SystemBaseUrl}{newsItem.Url}</link>
    <description>{newsItem.GetShortContent(10000)}</description>
    <pubDate>{newsItem.CreatedOn.AddHours(-3):ddd, dd MMM yyyy HH:mm:ss zzz}</pubDate>
    <category>{newsItem.SourceName}</category>
    <guid>{GlobalConstants.SystemBaseUrl}{newsItem.Url}</guid>
</item>");
            }

            sb.AppendLine("</channel>");
            sb.AppendLine("</rss>");
            return this.Content(sb.ToString(), "application/xml");
        }
    }
}
