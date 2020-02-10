namespace PressCenters.Services.Sources.BgNgos
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Институт за пазарна икономика.
    /// </summary>
    public class GallupInternationalBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.gallup-international.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("search-results/", ".glp-articles-list a.open-article", count: 6);

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[^2].Trim('/');
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.entry-title");
            var title = titleElement?.TextContent;
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var timeAsString = document.QuerySelector(".posted-on time.entry-date")?.Attributes["datetime"]?.Value;
            if (timeAsString == null)
            {
                return null;
            }

            var time = DateTime.Parse(timeAsString, CultureInfo.InvariantCulture);

            var contentElement = document.QuerySelector(".entry-content");
            if (contentElement == null)
            {
                throw new Exception("Content element is null");
            }

            var imageElement = contentElement?.QuerySelector("#content img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/gallup-international.bg.png";

            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
