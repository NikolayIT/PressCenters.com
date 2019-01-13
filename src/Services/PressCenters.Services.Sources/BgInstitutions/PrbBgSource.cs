namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public class PrbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.prb.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news/aktualno", ".list-field .list-content a", "bg/news/aktualno");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 1100; i++)
            {
                var news = this.GetPublications(
                    $"bg/news/aktualno?page={i}",
                    ".list-field .list-content a",
                    "bg/news/aktualno");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim());
            return uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1].Trim('/');
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var timeElement = document.QuerySelector(".article-title time");
            var titleElement = document.QuerySelector(".article-title");
            if (titleElement == null)
            {
                return null;
            }

            titleElement.RemoveRecursively(timeElement);
            var title = titleElement.TextContent.Trim();

            var timeAsString = timeElement.Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);

            var contentElement = document.QuerySelector(".narrow-content .text");
            if (contentElement == null)
            {
                return null;
            }

            contentElement.RemoveRecursively(titleElement);
            contentElement.RemoveRecursively(timeElement);
            contentElement.RemoveRecursively(document.QuerySelector(".tab-container"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".slide img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/prb.bg.jpg";

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
