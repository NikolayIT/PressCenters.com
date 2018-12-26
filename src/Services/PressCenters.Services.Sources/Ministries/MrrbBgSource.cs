namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Parser.Html;

    /// <summary>
    /// Министерство на регионалното развитие и благоустройството.
    /// </summary>
    public class MrrbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mrrb.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}bg/prescentur/novini/";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".category-articles .list-article a")
                .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl)).Distinct().ToList();
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 500; i++)
            {
                var address = $"{this.BaseUrl}bg/prescentur/novini/page/{i}/";
                var document = this.BrowsingContext.OpenAsync(address).Result;
                var links = document.QuerySelectorAll(".category-articles .list-article a")
                    .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl)).Distinct().ToList();
                var news = links.Select(this.GetPublication).ToList();
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector("h1.page-heading");
            var title = titleElement?.TextContent;

            var timeElement = document.QuerySelector("div.page-date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy | HH:mm", CultureInfo.GetCultureInfo("bg-BG"));

            var imageElement = document.QuerySelector(".mainImage img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/mrrb.bg.jpg";

            var contentElement = document.QuerySelector(".article-description");
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
