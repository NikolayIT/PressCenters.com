namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на външните работи.
    /// </summary>
    public class MfaBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mfa.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}bg/news";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".main-news .news-item h2 a")
                .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl))
                .Where(x => x.Contains("bg/news")).Distinct().ToList();
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 300; i++)
            {
                var address = $"{this.BaseUrl}bg/news?p={i}";
                var document = this.BrowsingContext.OpenAsync(address).Result;
                var links = document.QuerySelectorAll(".main-news .news-item h2 a")
                    .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl))
                    .Where(x => x.Contains("bg/news")).Distinct().ToList();
                var news = links.Select(this.GetPublication).ToList();
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
