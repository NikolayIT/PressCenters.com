namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    using PressCenters.Common;

    /// <summary>
    /// „ЧЕЗ България” ЕАД.
    /// </summary>
    public class CezBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.cez.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/novini/", "h2.margin-top-5 a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 105; page++)
            {
                var document = this.Parser.Parse(this.ReadStringFromUrl($"{this.BaseUrl}bg/novini/?pg={page}"));
                var newsElements = document.QuerySelectorAll(".first-child-no-margin-top .margin-bottom-20");
                var newsCount = 0;
                foreach (var newsElement in newsElements)
                {
                    var url = this.NormalizeUrl(newsElement.QuerySelector("a").Attributes["href"].Value);
                    var dateAsString = newsElement.QuerySelector(".day").TextContent.Trim() + " " +
                                       newsElement.QuerySelector(".month").TextContent.Trim() + " " +
                                       newsElement.QuerySelector(".year").TextContent.Trim();
                    var date = DateTime.ParseExact(dateAsString, "d MMMM yyyy", new CultureInfo("bg-BG"));
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

        internal override string ExtractIdFromUrl(string url) => url.GetLastStringBetween("/", ".html");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".p-content h1.white-background");
            var title = titleElement.TextContent.Trim();

            var imageElement = document.QuerySelector(".p-content div.padding-30 img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/cez.bg.png";

            var contentElement = document.QuerySelector(".p-content div.padding-30");
            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, DateTime.Now, imageUrl);
        }
    }
}
