namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    public class ConstCourtBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.constcourt.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var news = this.GetPublications("bg/news", ".page-content a", count: 10);
            var messages = this.GetPublications("bg/messages", ".page-content a", count: 10);
            return news.Concat(messages);
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".page-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".news-date");
            var timeAsString = timeElement.TextContent;
            if (!DateTime.TryParseExact(timeAsString.ToLower().Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            {
                time = DateTime.ParseExact(timeAsString.ToLower().Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            var contentElement = document.QuerySelector(".news-description");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".gallery-list img");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
