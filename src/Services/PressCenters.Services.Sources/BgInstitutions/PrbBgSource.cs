namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    using PressCenters.Common;

    public class PrbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://prb.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news/aktualno", ".news-box .news-group a", "bg/news/aktualno", 10);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 1350; i++)
            {
                var news = this.GetPublications(
                    $"bg/news/aktualno?p={i}",
                    ".news-box .news-group a",
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
            return uri.Segments[^2] + uri.Segments[^1].Trim('/');
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h3.title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".line--red");
            var timeAsString = timeElement.TextContent;
            var time = DateTime.ParseExact(timeAsString.ToLower(), "d MMMM yyyy г.", new CultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".publication-wrapper .col--8");
            if (contentElement == null)
            {
                return null;
            }

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".image-container .image");
            var imageUrl = imageElement?.GetAttribute("style").GetStringBetween("url('", "');");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
