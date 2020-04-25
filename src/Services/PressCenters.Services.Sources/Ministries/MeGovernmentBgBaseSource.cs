namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    using PressCenters.Common;

    /// <summary>
    /// Министерство на енергетиката.
    /// </summary>
    public abstract class MeGovernmentBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.me.government.bg/";

        protected abstract string NewsListUrl { get; }

        protected abstract int NewsListPagesCount { get; }

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(this.NewsListUrl, ".listing-article h2 a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= this.NewsListPagesCount; i++)
            {
                var page = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{{\"page\":{i}}}"));
                var news = this.GetPublications($"{this.NewsListUrl}?p={page}", ".listing-article h2 a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => url.GetLastStringBetween("-", ".html", url);

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("article.item-article h1.section-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector("header.article-header time");
            if (timeElement == null)
            {
                return null;
            }

            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector("div.article-image-container a.article-image");
            var imageUrl = imageElement?.GetAttribute("href");

            var contentElement = document.QuerySelector("article.item-article");
            contentElement.RemoveRecursively(document.QuerySelector("article.item-article .article-header"));
            contentElement.RemoveRecursively(document.QuerySelector("article.item-article .article-image-container"));
            contentElement.RemoveRecursively(document.QuerySelector("article.item-article .print-article"));
            contentElement.RemoveRecursively(document.QuerySelector("article.item-article .article-footer"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
