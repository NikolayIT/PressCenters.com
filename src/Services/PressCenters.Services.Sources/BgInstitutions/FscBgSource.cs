namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public class FscBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.fsc.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("?page_id=146", ".ps-live a", count: 10);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 570; i++)
            {
                var news = this.GetPublications($"paged={i}&page_id=146", ".ps-live a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        // Articles are reachable both via the legacy "?p=<id>" query and via pretty permalinks
        // (/<slug>/); take the id from whichever shape the URL uses.
        internal override string ExtractIdFromUrl(string url) =>
            url.Contains("?p=") ? this.GetUrlParameterValue(url, "p") : new Uri(url.Trim().Trim('/')).Segments[^1].Trim('/');

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h4.entry-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector("time.entry-date");
            var timeAsString = timeElement?.GetAttribute("datetime");
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.Parse(timeAsString);

            var imageElement = document.QuerySelector(".entry-content img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".entry-content");

            // Drop the lead image together with any <figure>/<a> wrapper so its URL does not linger in the body.
            contentElement.RemoveRecursively(imageElement?.Closest("figure, a") ?? imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
