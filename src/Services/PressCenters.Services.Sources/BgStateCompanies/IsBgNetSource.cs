namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// "Информационно обслужване" АД.
    /// </summary>
    public class IsBgNetSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://is-bg.net/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news", ".news a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            this.Headers = new List<(string Header, string Value)> { ("x-requested-with", "XMLHttpRequest") };
            for (var i = 1; i <= 25; i++)
            {
                var news = this.GetPublications($"bg/news?page={i}", ".news a");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }

                Console.WriteLine($"page {i} => {news.Count} news.");
            }

            this.Headers = null;
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".single-news-content h3.title");
            var title = titleElement.TextContent.Trim();

            var imageElement = document.QuerySelector(".news-image .big-img-wrapper img");
            var imageUrl = imageElement?.GetAttribute("src");

            var timeElement = document.QuerySelector(".single-news-content .date");
            var timeAsString = timeElement?.TextContent?.Trim().ToLower();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM, yyyy", new CultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".single-news-content .news-text");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
