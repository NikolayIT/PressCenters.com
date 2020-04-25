namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    using PressCenters.Common;

    /// <summary>
    /// УМБАЛСМ "Н.И.Пирогов".
    /// </summary>
    public class PirogovEuSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://pirogov.eu/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/novini_c61", ".main-content h4.news-card-title a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 12; page++)
            {
                var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}bg/novini_c61/{page}"));
                var newsElements = document.QuerySelectorAll(".card-section");
                var newsCount = 0;
                foreach (var newsElement in newsElements)
                {
                    var url = this.NormalizeUrl(newsElement.QuerySelector("h4.news-card-title a").Attributes["href"].Value);
                    var dateAsString = newsElement.QuerySelector(".news-card-date").TextContent.Trim();
                    var date = DateTime.ParseExact(dateAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    var remoteNews = this.GetPublication(url);
                    if (remoteNews == null)
                    {
                        continue;
                    }

                    newsCount++;
                    remoteNews.PostDate = date;
                    yield return remoteNews;
                }

                Console.WriteLine($"Page {page} => {newsCount} news");
            }
        }

        internal override string ExtractIdFromUrl(string url) => url.GetLastStringBetween("_p", ".html");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.mt30");
            var title = titleElement.TextContent.Trim();

            var imageElement = document.QuerySelector(".main-content .small-12 a.fancybox");
            var imageUrl = imageElement?.GetAttribute("href");

            var contentElement = document.QuerySelector(".main-content .small-12");
            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, DateTime.Now, imageUrl);
        }
    }
}
