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
            this.GetPublications("bg/publications/news", "a.news-card", count: 6);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            this.Headers = new List<(string Header, string Value)> { ("x-requested-with", "XMLHttpRequest") };
            for (var i = 1; i <= 25; i++)
            {
                var news = this.GetPublications($"bg/publications/news?page={i}", "a.news-card");
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
            var titleElement = document.QuerySelector("h1.page-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            // The lead image carries a ?w=&h= resize query; drop it so the stored URL is the full-size original.
            var imageElement = document.QuerySelector("img#anchor");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src")?.Split('?')[0]);

            var timeElement = document.QuerySelector(".page-content__meta");
            var timeAsString = timeElement?.TextContent?.Replace("Публикувано", string.Empty).Trim().ToLower();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy", new CultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".page-content__main");
            if (contentElement == null)
            {
                return null;
            }

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
