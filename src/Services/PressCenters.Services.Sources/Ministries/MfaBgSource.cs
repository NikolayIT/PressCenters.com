namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на външните работи.
    /// </summary>
    public class MfaBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mfa.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news", ".main-news .news-item h2 a", "bg/news");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 300; i++)
            {
                var news = this.GetPublications($"bg/news?p={i}", ".main-news .news-item h2 a", "bg/news");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector("h1.news-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector(".news-item .date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy", new CultureInfo("bg-BG"));

            var imageElement = document.QuerySelector(".news-item img.main-pic");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/mfa.bg.png";

            var contentElement = document.QuerySelector(".news-item .content");
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
