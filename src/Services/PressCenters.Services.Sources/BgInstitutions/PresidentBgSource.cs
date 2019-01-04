namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    using PressCenters.Common;

    public class PresidentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.president.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}news/";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".inside-article-box a.dblock").Select(
                x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl)).ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 22; i++)
            {
                var address = $"{this.BaseUrl}news/all/{i}";
                var document = this.BrowsingContext.OpenAsync(address).Result;
                var links = document.QuerySelectorAll(".inside-article-box a.dblock").Select(
                    x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl)).ToList();
                var news = links.Select(this.GetPublication).ToList();
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        public override string ExtractIdFromUrl(string originalUrl) => originalUrl?.GetStringBetween("/news", "/");

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector(".print-content h2");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".print-content .date");
            var timeAsString = timeElement.TextContent;
            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy | HH:mm", CultureInfo.GetCultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".print-content .index-news-bdy");
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".print-content img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/president.bg.jpg";

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
