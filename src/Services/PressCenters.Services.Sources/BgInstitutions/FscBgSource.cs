namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public class FscBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.fsc.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("?page_id=146", ".ps-live a", urlShouldContain: "?p=");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 570; i++)
            {
                var news = this.GetPublications($"paged={i}&page_id=146", ".ps-live a", urlShouldContain: "?p=");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => this.GetUrlParameterValue(url, "p");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h4.entry-title");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector("time.entry-date");
            var timeAsString = timeElement.Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);

            var imageElement = document.QuerySelector(".entry-content img");
            var imageUrl = imageElement?.Attributes?["src"]?.Value;

            var contentElement = document.QuerySelector(".entry-content");
            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
