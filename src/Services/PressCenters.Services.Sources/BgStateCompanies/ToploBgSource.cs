namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    public class ToploBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://toplo.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("/", ".news a", count: 6);

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim('/'));
            if (uri.Segments.Length == 3 || uri.Segments.Length == 2)
            {
                return uri.Segments[^1];
            }

            return uri.Segments[^4] + uri.Segments[^3] + uri.Segments[^2] + uri.Segments[^1];
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.title");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".news-content .date");
            var timeAsString = timeElement?.TextContent?.ToLower()?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy", CultureInfo.GetCultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".news-content");
            contentElement.RemoveRecursively(contentElement.QuerySelector(".title"));
            contentElement.RemoveRecursively(contentElement.QuerySelector(".date"));
            contentElement.RemoveRecursively(contentElement.QuerySelector(".read-more"));

            var imageElement = contentElement.QuerySelector("img");
            var imageUrl = imageElement?.GetAttribute("src");

            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
