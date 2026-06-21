namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;

    using AngleSharp.Dom;

    public abstract class MvrBgBaseSource : BaseSource
    {
        public override bool UseProxy => true;

        // mvr.bg blocks .NET's direct fetch and the Azure relay IPs; only the Cloudflare relay reaches it,
        // and that relay accepts .NET only over HTTP/2.
        public override bool UseHttp2 => true;

        public override string BaseUrl { get; } = "https://www.mvr.bg/";

        public abstract string NewsListUrl { get; }

        public virtual string NewsLinkSelector => "a.card__title";

        public abstract int NewsListPagesCount { get; }

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(this.NewsListUrl, this.NewsLinkSelector, count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= this.NewsListPagesCount; i++)
            {
                var news = this.GetPublications($"{this.NewsListUrl}?p={i}", this.NewsLinkSelector, throwOnEmpty: false);
                Console.WriteLine($"№{i} => {news.Count} news");
                if (news.Count == 0)
                {
                    break;
                }

                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            // Last 2 url segments (e.g. "новини/91393"), matching the historical RemoteId format.
            var uri = new Uri(url.Trim().Trim('/'));
            return WebUtility.UrlDecode(uri.Segments[^2] + uri.Segments[^1]);
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.page-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".page-content__header small");
            var timeAsString = timeElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy", CultureInfo.GetCultureInfo("bg-BG"));

            // The lead image carries a ?w= resize query; drop it to store the original.
            var imageElement = document.QuerySelector(".page-gallery img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src")?.Split('?')[0]);

            var contentElement = document.QuerySelector(".page-content");
            if (contentElement == null)
            {
                return null;
            }

            contentElement.RemoveRecursively(document.QuerySelector(".page-gallery"));
            contentElement.RemoveRecursively(document.QuerySelector(".share-widget"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
