namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;

    using AngleSharp.Dom;

    using PressCenters.Common;

    /// <summary>
    /// Комисия за енергийно и водно регулиране (КЕВР).
    /// </summary>
    public class DkerBgSource : BaseSource
    {
        public override string BaseUrl => "https://www.dker.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/novini.html", ".NewsSummaryLink a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 92; page++)
            {
                var news = this.GetPublications(
                    $"index.php?mact=News,m14562,default,1&m14562category=news&m14562summarytemplate=news&m14562number=8&m14562detailpage=65&m14562pagenumber={page}&m14562returnid=65&page=65",
                    ".NewsSummaryLink a");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }

                Console.WriteLine($"Page {page} => {news.Count} news");
            }
        }

        internal override string ExtractIdFromUrl(string url) =>
            WebUtility.UrlDecode(url.GetLastStringBetween("/", ".html"));

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("#NewsPostDetailTitle");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector("#NewsPostDetailDate");
            var time = DateTime.ParseExact(timeElement?.TextContent?.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".NewsDetailField img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector("#NewsPostDetailContent");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
