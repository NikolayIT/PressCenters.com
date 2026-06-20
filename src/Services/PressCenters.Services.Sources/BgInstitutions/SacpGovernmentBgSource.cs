namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Държавна агенция за закрила на детето.
    /// </summary>
    public class SacpGovernmentBgSource : BaseSource
    {
        public override string BaseUrl => "https://sacp.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("news", ".product-thumb .image a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 0; i <= 92; i++)
            {
                var news = this.GetPublications($"news?page={i}", ".product-thumb .image a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("#content h1") ?? document.QuerySelector("h1");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".name-sub-title");
            var timeAsString = timeElement?.TextContent?.Replace("г.", string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".thumbnails img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector("#tab-description");
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
