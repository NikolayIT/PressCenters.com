namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Комисия за защита на личните данни. Migrated from the old www.cpdp.bg PHP site to WordPress on
    /// cpdp.bg; news live at /category/новини/. The relays cannot reach the new site (526 SSL / 502), so it
    /// is fetched directly (no proxy).
    /// </summary>
    public class CpdpBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://cpdp.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("category/новини/", ".entry-title a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 200; page++)
            {
                var address = page == 1 ? "category/новини/" : $"category/новини/page/{page}/";
                var news = this.GetPublications(address, ".entry-title a", throwOnEmpty: false);
                Console.WriteLine($"Page {page} => {news.Count} news");
                if (news.Count == 0)
                {
                    yield break;
                }

                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var title = document.QuerySelector("meta[property='og:title']")?.GetAttribute("content")?.Trim();
            var timeAsString = document.QuerySelector("meta[property='article:published_time']")?.GetAttribute("content");
            var contentElement = document.QuerySelector(".entry-content");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(timeAsString) || contentElement == null)
            {
                return null;
            }

            var time = DateTimeOffset.Parse(timeAsString, CultureInfo.InvariantCulture).UtcDateTime;
            var imageUrl = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content");

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
