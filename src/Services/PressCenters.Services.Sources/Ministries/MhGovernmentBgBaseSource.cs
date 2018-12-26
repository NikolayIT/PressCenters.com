namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    public abstract class MhGovernmentBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mh.government.bg/";

        protected abstract string NewsListUrl { get; }

        protected abstract int NewsListPagesCount { get; }

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var document = this.BrowsingContext.OpenAsync(this.NewsListUrl).Result;
            var links = document.QuerySelectorAll(".news h2 a").Select(x => x.Attributes["href"].Value)
                .Select(x => this.NormalizeUrl(x, this.BaseUrl)).ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= this.NewsListPagesCount; i++)
            {
                Console.WriteLine(i);
                var address = $"{this.NewsListUrl}?page={i}";
                var document = this.BrowsingContext.OpenAsync(address).Result;

                var links = document.QuerySelectorAll(".news h2 a").Select(x => x.Attributes["href"].Value)
                    .Select(x => this.NormalizeUrl(x, this.BaseUrl)).ToList();
                var news = links.Select(this.GetPublication).ToList();
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        public override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1];
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var title = document.QuerySelector("h1").TextContent.Trim();

            var imageElement = document.QuerySelector(".carousel-inner .active img");
            var imageUrl = imageElement?.GetAttribute("src") ?? $"/images/sources/mh.government.bg.jpg";

            var contentElement = document.QuerySelector(".single_news");
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement.InnerHtml;

            var documentElement = document.QuerySelector(".single_news + .panel");
            if (documentElement != null)
            {
                this.NormalizeUrlsRecursively(documentElement, this.BaseUrl);
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
