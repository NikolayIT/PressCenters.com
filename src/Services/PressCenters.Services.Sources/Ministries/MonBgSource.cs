namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на образованието и науката.
    /// </summary>
    public class MonBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mon.bg/";

        // mon.bg blocks .NET's direct fetch and the Azure relay IPs; only the Cloudflare relay reaches it,
        // and that relay needs HTTP/2.
        public override bool UseProxy => true;

        public override bool UseHttp2 => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("novini", "a.card-link", "/news/", 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 300; i++)
            {
                var news = this.GetPublications($"novini/page/{i}", "a.card-link", "/news/", throwOnEmpty: false);
                Console.WriteLine($"Page {i} => {news.Count} news");
                if (news.Count == 0)
                {
                    break;
                }

                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.post-title");
            if (titleElement == null)
            {
                return null;
            }

            // The site renders titles in all-caps; normalise to title case (matches the legacy articles).
            var title = new CultureInfo("bg-BG", false).TextInfo.ToTitleCase(
                (titleElement.TextContent?.Trim() ?? string.Empty).ToLower());

            var timeElement = document.QuerySelector(".post-date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy", new CultureInfo("bg-BG"));

            var imageElement = document.QuerySelector(".post-thumbnail img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".entry-content");
            if (contentElement == null)
            {
                return null;
            }

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
