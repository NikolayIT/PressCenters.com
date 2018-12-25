namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;

    public class ToploBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://toplo.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}news";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".post a")
                .Select(x => this.NormalizeUrl(x.Attributes["href"]?.Value, this.BaseUrl)).ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim('/'));
            return uri.Segments[uri.Segments.Length - 4] +
                   uri.Segments[uri.Segments.Length - 3] +
                   uri.Segments[uri.Segments.Length - 2] +
                   uri.Segments[uri.Segments.Length - 1];
        }

        protected override RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".l9 .card-title strong");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".l9 .post_author_date .post_content_date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM, yyyy", CultureInfo.GetCultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".l9 .card-content .card-content");
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".l9 .card-image img.img-blog");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
