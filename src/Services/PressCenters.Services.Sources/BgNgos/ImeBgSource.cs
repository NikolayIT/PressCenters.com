namespace PressCenters.Services.Sources.BgNgos
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    /// <summary>
    /// Институт за пазарна икономика.
    /// </summary>
    public class ImeBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://ime.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("articles", "article a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 304; i <= 917; i++)
            {
                var news = this.GetPublications($"bg/pr_bg/issues/{i}/", "h3 a", "bg/articles");
                Console.WriteLine($"№{i} => {news.Count} news ({news.DefaultIfEmpty().Min(x => x?.PostDate)} - {news.DefaultIfEmpty().Max(x => x?.PostDate)})");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.the-title");
            var title = titleElement?.TextContent;
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var timeElement = document.QuerySelector(".single-post__the-content-author-information span");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var contentElement = document.QuerySelector(".large-9");
            contentElement.RemoveRecursively(titleElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var imageElement = document.QuerySelector(".featured-image img");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
