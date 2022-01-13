namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    public class SofiyskaVodaBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.sofiyskavoda.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("novini", ".news-item__title a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 5; i++)
            {
                var news = this.GetPublications($"novini?page={i}", ".news-item__title a");
                Console.WriteLine($"№{i} => {news.Count} news.");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".title");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".date");
            var timeAsString = timeElement?.TextContent?.ToLower()?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".text__hero img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".dynamic-cms-text");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
