namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на регионалното развитие и благоустройството.
    /// </summary>
    public class MrrbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mrrb.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/prescentur/novini/", ".category-articles .list-article a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 500; i++)
            {
                var news = this.GetPublications(
                    $"bg/prescentur/novini/page/{i}/",
                    ".category-articles .list-article a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.page-heading");
            var title = titleElement?.TextContent;

            var timeElement = document.QuerySelector("div.page-date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy | HH:mm", CultureInfo.GetCultureInfo("bg-BG"));

            var imageElement = document.QuerySelector(".mainImage img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/mrrb.bg.jpg";

            var contentElement = document.QuerySelector(".article-description");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
