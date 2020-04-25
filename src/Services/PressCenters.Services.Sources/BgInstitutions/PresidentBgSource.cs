namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    using PressCenters.Common;

    public class PresidentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.president.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("news/", ".inside-article-box a.dblock", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 22; i++)
            {
                var news = this.GetPublications($"news/all/{i}", ".inside-article-box a.dblock");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string originalUrl) => originalUrl?.GetStringBetween("/news", "/");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".print-content h2");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".print-content .date");
            var timeAsString = timeElement.TextContent;
            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy | HH:mm", CultureInfo.GetCultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".print-content .index-news-bdy");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".print-content img");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
