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
            var title = titleElement.TextContent.Trim();

            var timeAsString = document.QuerySelector("meta[property='article:published_time']").GetAttribute("content");
            var time = DateTime.Parse(timeAsString, CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector("meta[property='og:image']");
            var imageUrl = imageElement?.GetAttribute("content");

            var contentElement = document.QuerySelector(".post-content");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
