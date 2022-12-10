namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    /// <summary>
    /// Комисия за защита на конкуренцията.
    /// </summary>
    public class CpcBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.cpc.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("news", ".news-summary-link");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 30; page++)
            {
                var news = this.GetPublications($"news?page={page}", ".news-summary-link");
                Console.WriteLine($"Page {page} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    remoteNews.OriginalUrl = remoteNews.OriginalUrl.Split('?')[0];
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => new Uri(url.Trim().Trim('/')).Segments[^1].Split('-')[1];

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.news-detail-title");
            var title = titleElement.TextContent.Trim();

            var day = document.QuerySelector(".day").TextContent.Trim();
            var month = document.QuerySelector(".month").TextContent.Trim();
            var year = document.QuerySelector(".year").TextContent.Trim();
            var time = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

            var contentElement = document.QuerySelector(".news-detail-content");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, null);
        }
    }
}
