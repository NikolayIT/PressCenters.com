namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AngleSharp.Dom;
    using PressCenters.Common;

    public class VksBgSource : BaseSource
    {
        public override string BaseUrl => "http://www.vks.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
            => this.GetPublications("novini.html", "#Content p a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
            => this.GetPublications("novini.html", "#Content p a");

        internal override string ExtractIdFromUrl(string url) => url.GetLastStringBetween("/", ".html", url);

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("#Content h2");
            var title = titleElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var timeElement = document.QuerySelector("#Content time");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var contentElement = document.QuerySelector("#Content");
            contentElement.RemoveRecursively(document.QuerySelector("#Content .fa-facebook"));
            contentElement.RemoveRecursively(document.QuerySelector("#Content .fa-twitter"));
            contentElement.RemoveRecursively(titleElement);
            contentElement.RemoveRecursively(timeElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, null);
        }
    }
}
