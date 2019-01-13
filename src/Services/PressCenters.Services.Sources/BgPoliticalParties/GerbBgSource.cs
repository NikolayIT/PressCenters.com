namespace PressCenters.Services.Sources.BgPoliticalParties
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    using PressCenters.Common;

    public class GerbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://gerb.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news/spisyk-novini-1.html", "#container-main-ajax article p a");

        internal override string ExtractIdFromUrl(string url) =>
            url.GetStringBetween("-", ".html", url.LastIndexOf("-", StringComparison.InvariantCulture));

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector(".news-info h1");
            var title = titleElement.TextContent.Trim();

            var dateAsString = document.QuerySelector(".news-info .date").InnerHtml;
            var time = DateTime.ParseExact(dateAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            time = time.Date == DateTime.UtcNow.Date ? DateTime.Now : time;

            var contentElement = document.QuerySelector(".news-info");
            this.NormalizeUrlsRecursively(contentElement);
            contentElement.RemoveRecursively(document.QuerySelector(".news-info .social"));
            contentElement.RemoveRecursively(document.QuerySelector(".news-info .date"));
            contentElement.RemoveRecursively(document.QuerySelector(".news-info h1"));
            contentElement.RemoveRecursively(document.QuerySelector(".news-info .more"));
            contentElement.RemoveRecursively(document.QuerySelector(".news-info .social-r"));
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector("#slider img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/gerb.bg.jpg";

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
