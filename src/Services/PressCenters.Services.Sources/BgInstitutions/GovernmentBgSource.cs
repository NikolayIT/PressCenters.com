namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;

    public class GovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}bg/prestsentar/novini";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".articles .item a").Select(
                x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl)).Distinct().ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                         ? uri.Segments[uri.Segments.Length - 1].Trim('/')
                         : uri.Segments[uri.Segments.Length - 2].Trim('/');
            return id;
        }

        protected override RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".view h1");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".view p");
            var timeAsString = timeElement.TextContent;
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".view .gallery img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".view");
            this.RemoveRecursively(contentElement, titleElement);
            this.RemoveRecursively(contentElement, timeElement);
            this.RemoveRecursively(contentElement, document.QuerySelector(".view .gallery"));
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
