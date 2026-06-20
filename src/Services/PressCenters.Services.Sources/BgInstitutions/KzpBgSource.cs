namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Комисия за защита на потребителите.
    /// </summary>
    public class KzpBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://kzp.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/novini", "a.news-card", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 145; page++)
            {
                var news = this.GetPublications($"bg/novini?page={page}", "a.news-card");
                Console.WriteLine($"Page {page} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.page-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".page-meta__data");
            var timeAsString = timeElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy", new CultureInfo("bg-BG"));

            // The lead image carries a ?w= resize query; drop it so the stored URL is the full-size original.
            var imageElement = document.QuerySelector(".page-figure__image img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src")?.Split('?')[0]);

            var contentElement = document.QuerySelector(".page-content");
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
