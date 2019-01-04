namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    public class PrbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.prb.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}bg/news/aktualno";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".list-field .list-content a")
                .Select(x => x.Attributes?["href"]?.Value).Select(x => this.NormalizeUrl(x, this.BaseUrl))
                .Where(x => x.Contains("bg/news/aktualno")).Distinct().ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 1100; i++)
            {
                var address = $"{this.BaseUrl}bg/news/aktualno?page={i}";
                var document = this.BrowsingContext.OpenAsync(address).Result;
                var links = document.QuerySelectorAll(".list-field .list-content a")
                    .Select(x => x.Attributes?["href"]?.Value).Select(x => this.NormalizeUrl(x, this.BaseUrl))
                    .Where(x => x.Contains("bg/news/aktualno")).Distinct().ToList();
                var news = links.Select(this.GetPublication).ToList();
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        public override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim());
            return uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1].Trim('/');
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var timeElement = document.QuerySelector(".article-title time");
            var titleElement = document.QuerySelector(".article-title");
            this.RemoveRecursively(titleElement, timeElement);
            var title = titleElement.TextContent.Trim();

            var timeAsString = timeElement.Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);

            var contentElement = document.QuerySelector(".narrow-content .text");
            this.RemoveRecursively(contentElement, titleElement);
            this.RemoveRecursively(contentElement, timeElement);
            this.RemoveRecursively(contentElement, document.QuerySelector(".tab-container"));
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".slide img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/prb.bg.jpg";

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
