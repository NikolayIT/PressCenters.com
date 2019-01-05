namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;
    using AngleSharp.Extensions;

    public class FscBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.fsc.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/novini/", ".news-box-listing a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 400; i++)
            {
                var news = this.GetPublications($"bg/novini/?p={i}", ".news-box-listing a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        public override string ExtractIdFromUrl(string url)
        {
            const string EndString = ".html";
            if (string.IsNullOrWhiteSpace(url))
            {
                return "0";
            }

            var startIndex = url.LastIndexOf("-", StringComparison.Ordinal);
            if (startIndex == -1)
            {
                return string.Empty;
            }

            var endIndex = url.LastIndexOf(EndString, StringComparison.Ordinal);
            if (endIndex == -1)
            {
                return string.Empty;
            }

            return url.Substring(startIndex + 1, url.Length - EndString.Length - startIndex - 1);
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector("#content-left-inner h2");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector("#content-left-inner .article_date");
            var time = DateTime.ParseExact(timeElement.TextContent, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            time = time.Date == DateTime.UtcNow.Date ? DateTime.Now : time;

            var imageUrl = document.QuerySelector("#content-right img")?.Attributes?["src"]?.Value
                           ?? "http://www.fsc.bg/_assets/img/banner.jpg";

            var contentElement = document.QuerySelector("#content-left-inner");
            contentElement.RemoveElement(titleElement);
            contentElement.RemoveElement(timeElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
