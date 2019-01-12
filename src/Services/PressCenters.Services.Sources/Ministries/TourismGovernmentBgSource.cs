namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на туризма.
    /// </summary>
    public class TourismGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.tourism.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/kategorii/novini", "#main .node-article h2 a", "bg/kategorii/novini");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 0; i <= 105; i++)
            {
                var news = this.GetPublications(
                    $"bg/kategorii/novini?page={i}",
                    "#main .node-article h2 a",
                    "bg/kategorii/novini");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        public override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1];
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector("#main h1");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector("#main .content span.date-display-single");
            var time = DateTime.Parse(timeElement?.Attributes["content"]?.Value, CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector("#main .content .field-name-field-image a");
            var imageUrl = imageElement?.GetAttribute("href") ?? "/images/sources/tourism.government.bg.jpg";

            var contentElement = document.QuerySelector("#main .content .field-name-body .field-item");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
