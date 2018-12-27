namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на образованието и науката.
    /// </summary>
    public class MonBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mon.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}bg/news";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".col-md-9 .news-description a")
                .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl))
                .Where(x => x.Contains("bg/news"))
                .Distinct().ToList();
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 300; i++)
            {
                var address = $"{this.BaseUrl}bg/news?p={i}";
                var document = this.BrowsingContext.OpenAsync(address).Result;
                var links = document.QuerySelectorAll(".col-md-9 .news-description a")
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
            var titleElement = document.QuerySelector(".col-md-9.content-center h3");
            if (titleElement == null)
            {
                return null;
            }

            var title = new CultureInfo("bg-BG", false).TextInfo.ToTitleCase(
                titleElement?.TextContent?.ToLower() ?? string.Empty);

            var timeElement = document.QuerySelector(".col-md-9.content-center p");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".col-md-9.content-center img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/mon.bg.png";

            var socialMediaShareElement = document.QuerySelector(".col-md-9.content-center div");
            var contentElement = document.QuerySelector(".col-md-9.content-center");
            this.RemoveRecursively(contentElement, titleElement);
            this.RemoveRecursively(contentElement, timeElement);
            this.RemoveRecursively(contentElement, imageElement);
            this.RemoveRecursively(contentElement, socialMediaShareElement);
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
