namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на транспорта, информационните технологии и съобщенията.
    /// </summary>
    public class MtitcGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mtitc.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/category/1", "#main .views-field-title a", "bg/category/1");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 0; i <= 25; i++)
            {
                var news = this.GetPublications(
                    $"bg/category/1?page={i}",
                    "#main .views-field-title a",
                    "bg/category/1");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
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
            var imageUrl = imageElement.GetAttribute("href");

            var contentElement = document.QuerySelector("#main .content .field-name-body .field-item");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
