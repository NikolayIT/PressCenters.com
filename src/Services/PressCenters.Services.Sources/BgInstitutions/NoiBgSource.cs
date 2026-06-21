namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Национален осигурителен институт.
    /// </summary>
    public class NoiBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://nssi.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("publichnost/novini/", ".post-article h4 a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 25; page++)
            {
                // Page 1 is the bare listing URL; /page/1/ returns nothing.
                var address = page == 1 ? "publichnost/novini/" : $"publichnost/novini/page/{page}/";
                var remoteNews = this.GetPublications(address, ".post-article h4 a", throwOnEmpty: false);
                Console.WriteLine($"Page {page} => {remoteNews.Count}");
                if (remoteNews.Count == 0)
                {
                    yield break;
                }

                foreach (var news in remoteNews)
                {
                    yield return news;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("#main-content h1");
            var timeElement = document.QuerySelector("meta[property='article:published_time']");
            var contentElement = document.QuerySelector(".post-content");
            if (titleElement == null || timeElement == null || contentElement == null)
            {
                // A malformed/non-article page (e.g. an old archived item) should be skipped, not crash the run.
                return null;
            }

            var title = titleElement.TextContent.Trim();
            var time = DateTime.Parse(timeElement.GetAttribute("content"), CultureInfo.InvariantCulture);
            var imageUrl = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content");

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
