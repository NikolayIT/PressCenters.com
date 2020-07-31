namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;

    using AngleSharp.Dom;

    using PressCenters.Common;

    public class ConstCourtBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.constcourt.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/Blog/AllMessages?page=1&pageSize=5", ".row-title a", "/bg/Blog/Display/", 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 102; i++)
            {
                var news = this.GetPublications(
                    $"bg/Blog/AllMessages?page={i}&pageSize=5",
                    ".row-title a",
                    "/bg/Blog/Display/");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                    Thread.Sleep(2000);
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".case-details-heading");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".heading-title-all");
            var timeAsString = timeElement.TextContent;
            var time = DateTime.ParseExact(timeAsString.ToLower().Trim(), "d MMMM yyyy г.", new CultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".blog-content");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, null);
        }
    }
}
