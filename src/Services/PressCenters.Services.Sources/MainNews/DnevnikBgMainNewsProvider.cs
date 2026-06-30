namespace PressCenters.Services.Sources.MainNews
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    public class DnevnikBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.dnevnik.bg";

        public override RemoteMainNews GetMainNews()
        {
            // dnevnik.bg's HTML pages sit behind Cloudflare bot-protection that 403s the server's HttpClient
            // (regardless of HTTP version or headers, and the relays' egress IPs are blocked too). Its RSS feed
            // is not protected, so read that instead -- the first <item> is the latest story.
            var feed = XDocument.Parse(this.GetContent("https://www.dnevnik.bg/rss/"));
            var item = feed.Descendants("item").FirstOrDefault();
            if (item == null)
            {
                return null;
            }

            var title = item.Element("title")?.Value?.Trim();

            var link = item.Element("link")?.Value?.Trim();
            if (!string.IsNullOrEmpty(link))
            {
                link = new Uri(link).GetLeftPart(UriPartial.Path); // drop the ?ref=rss tracking query
            }

            // The article thumbnail is the first <img> inside the (HTML-encoded) <description>.
            var imageUrl = ExtractFirstImageUrl(item.Element("description")?.Value);

            return new RemoteMainNews(title, this.MakeAbsoluteUrl(link), this.MakeAbsoluteUrl(imageUrl));
        }

        private static string ExtractFirstImageUrl(string descriptionHtml)
        {
            if (string.IsNullOrWhiteSpace(descriptionHtml))
            {
                return null;
            }

            var match = Regex.Match(descriptionHtml, "<img[^>]+src=\"([^\"]+)\"", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
