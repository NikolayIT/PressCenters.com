namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;

    using AngleSharp.Dom;
    using PressCenters.Common;

    public class ApiBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://api.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/novini", ".news-panel a", count: 5);

        internal override string ExtractIdFromUrl(string url) =>
            WebUtility.UrlDecode(url.GetLastStringBetween("/", ".html"));

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var contentElement = document.QuerySelector("#single-news");
            var titleElement = contentElement.QuerySelector("h1");
            var title = titleElement?.TextContent?.Trim();
            if (title == null)
            {
                return null;
            }

            var timeElement = contentElement.QuerySelector(".date");
            var time = DateTime.ParseExact(timeElement?.TextContent?.Trim(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

            var imageElement = contentElement.QuerySelector("img");
            var imageUrl = imageElement?.GetAttribute("src");

            contentElement.RemoveRecursively(contentElement.QuerySelector(".news-main-image"));
            contentElement.RemoveRecursively(titleElement);
            contentElement.RemoveRecursively(timeElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
