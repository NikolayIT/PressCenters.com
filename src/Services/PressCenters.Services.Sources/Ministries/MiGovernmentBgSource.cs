namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    using PressCenters.Common;

    /// <summary>
    /// Министерство на икономиката и индустрията.
    /// </summary>
    public class MiGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mi.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("novini/novini/", "h2.post-title span");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".post-title");
            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector(".post-date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".post-thumbnail img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".entry-content");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
