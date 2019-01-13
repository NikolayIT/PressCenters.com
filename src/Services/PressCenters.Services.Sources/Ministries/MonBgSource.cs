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

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news", ".col-md-9 .news-description a", "bg/news");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 300; i++)
            {
                var news = this.GetPublications($"bg/news?p={i}", ".col-md-9 .news-description a", "bg/news");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
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
            contentElement.RemoveRecursively(titleElement);
            contentElement.RemoveRecursively(timeElement);
            contentElement.RemoveRecursively(imageElement);
            contentElement.RemoveRecursively(socialMediaShareElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
