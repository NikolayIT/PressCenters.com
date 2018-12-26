namespace PressCenters.Services.Sources.BgPoliticalParties
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    using PressCenters.Common;

    public class GerbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://gerb.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}bg/news/spisyk-novini-1.html";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll("#container-main-ajax article p a")
                .Select(x => x.Attributes["href"].Value).Select(x => this.NormalizeUrl(x, this.BaseUrl)).ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override string ExtractIdFromUrl(string url)
        {
            var lastDash = url.LastIndexOf("-", StringComparison.InvariantCulture);
            var id = url.GetStringBetween("-", ".html", lastDash);
            return id;
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector(".news-info h1");
            var title = titleElement.TextContent.Trim();

            var dateAsString = document.QuerySelector(".news-info .date").InnerHtml;
            var time = DateTime.ParseExact(dateAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            time = time.Date == DateTime.UtcNow.Date ? DateTime.Now : time;

            var contentElement = document.QuerySelector(".news-info");
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info .social"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info .date"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info h1"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info .more"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info .social-r"));
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector("#slider img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/gerb.bg.jpg";

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
