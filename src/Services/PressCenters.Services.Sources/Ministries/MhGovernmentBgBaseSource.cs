namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public abstract class MhGovernmentBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mh.government.bg/";

        protected abstract string NewsListUrl { get; }

        protected abstract int NewsListPagesCount { get; }

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(this.NewsListUrl, ".news h2 a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= this.NewsListPagesCount; i++)
            {
                var news = this.GetPublications($"{this.NewsListUrl}?page={i}", ".news h2 a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1];
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var title = document.QuerySelector("h1").TextContent.Trim();

            var imageElement = document.QuerySelector(".carousel-inner .active img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/mh.government.bg.jpg";

            var contentElement = document.QuerySelector(".single_news");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml;

            var documentElement = document.QuerySelector(".single_news + .panel");
            if (documentElement != null)
            {
                this.NormalizeUrlsRecursively(documentElement);
                content += documentElement.InnerHtml;
            }

            var timeAsString = document.QuerySelector(".newsdate li time").Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);
            if (time.Minute == 0 && (time.Hour == 2 || time.Hour == 3))
            {
                time = time.Date;
            }

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
