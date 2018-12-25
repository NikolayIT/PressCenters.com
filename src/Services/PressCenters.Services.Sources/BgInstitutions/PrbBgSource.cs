namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp;

    public class PrbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.prb.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}bg/news/aktualno";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".list-field .list-content a")
                .Select(x => x.Attributes?["href"]?.Value).Select(x => this.NormalizeUrl(x, this.BaseUrl))
                .ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                         ? uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1].Trim('/')
                         : uri.Segments[uri.Segments.Length - 3] + uri.Segments[uri.Segments.Length - 2].Trim('/');
            return id;
        }

        protected override RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var timeElement = document.QuerySelector(".article-title time");
            var titleElement = document.QuerySelector(".article-title");
            this.RemoveRecursively(titleElement, timeElement);
            var title = titleElement.TextContent.Trim();

            var timeAsString = timeElement.Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);

            var contentElement = document.QuerySelector(".narrow-content .text");
            this.RemoveRecursively(contentElement, titleElement);
            this.RemoveRecursively(contentElement, timeElement);
            this.RemoveRecursively(contentElement, document.QuerySelector(".tab-container"));
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".slide img");
            var imageUrl = imageElement?.GetAttribute("src");

            var news = new RemoteNews
                       {
                           Title = title,
                           Content = content,
                           PostDate = time,
                           ImageUrl = imageUrl,
                       };
            return news;
        }
    }
}
