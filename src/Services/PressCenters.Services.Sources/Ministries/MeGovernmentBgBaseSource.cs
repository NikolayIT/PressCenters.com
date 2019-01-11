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

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(this.NewsListUrl, ".listing-article h2 a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 400; i <= 2700; i++)
            {
                Console.WriteLine(i);
                var remoteNews = this.GetPublication($"{this.BaseUrl}bg/news/news-{i}.html");
                if (remoteNews == null)
                {
                    continue;
                }

                Console.WriteLine($"News {i} => {remoteNews.Title}");
                yield return remoteNews;
            }
        }


        public override string ExtractIdFromUrl(string url) => url.GetLastStringBetween("-", ".html", url);

        protected override RemoteNews ParseDocument(IDocument document)
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
            var imageUrl = imageElement?.GetAttribute("href") ?? "/images/sources/me.government.bg.png";

            var contentElement = document.QuerySelector("article.item-article");
            contentElement.RemoveRecursively(document.QuerySelector("article.item-article .article-header"));
            contentElement.RemoveRecursively(document.QuerySelector("article.item-article .article-image-container"));
            contentElement.RemoveRecursively(document.QuerySelector("article.item-article .print-article"));
            contentElement.RemoveRecursively(document.QuerySelector("article.item-article .article-footer"));
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
