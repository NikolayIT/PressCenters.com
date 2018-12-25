namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    public abstract class MhGovernmentBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.mh.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = this.GetNewsListUrl();
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".news h2 a").Select(x => x.Attributes["href"].Value)
                .Select(x => this.NormalizeUrl(x, this.BaseUrl)).ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                         ? uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1].Trim('/')
                         : uri.Segments[uri.Segments.Length - 3] + uri.Segments[uri.Segments.Length - 2].Trim('/');
            return id;
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var title = document.QuerySelector("h1").TextContent.Trim();
            var imageUrl = $"{this.BaseUrl}static/images/Ministry_of_Health-heraldic.95741c3e92f7.svg";

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

            return new RemoteNews(title, content, time, imageUrl);
        }

        protected abstract string GetNewsListUrl();
    }
}
