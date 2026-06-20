namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    using PressCenters.Common;

    public class PrbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://prb.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news/aktualno", ".news-item__title a", "bg/news/aktualno", 10);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 1350; i++)
            {
                var news = this.GetPublications(
                    $"bg/news/aktualno?p={i}",
                    ".news-item__title a",
                    "bg/news/aktualno");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim());
            return uri.Segments[^2] + uri.Segments[^1].Trim('/');
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.page-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".publication-title__meta");
            var timeAsString = timeElement?.TextContent?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy 'г.'", new CultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".page-content");
            if (contentElement == null)
            {
                return null;
            }

            // The lead image is a (gallery) figure inside the content; pull it out, then drop it and any
            // resize query string so the stored content is text-only and the image URL is the full-size original.
            var imageElement = contentElement.QuerySelector(".media-container__figure img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src")?.Split('?')[0]);
            contentElement.RemoveRecursively(contentElement.QuerySelector(".media-container"));

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
