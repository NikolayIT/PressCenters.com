namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;

    public class GovernmentBgSource : BaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = "http://www.government.bg/bg/prestsentar/novini";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".articles .item a").Select(
                x => this.NormalizeUrl(x.Attributes["href"].Value, "http://www.government.bg/")).Distinct().ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            return news;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".view h1");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".view p");
            var timeAsString = timeElement.TextContent;
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".view .gallery img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "http://www.government.bg/").Trim();

            var contentElement = document.QuerySelector(".view");
            this.RemoveRecursively(contentElement, titleElement);
            this.RemoveRecursively(contentElement, timeElement);
            this.RemoveRecursively(contentElement, document.QuerySelector(".view .gallery"));
            this.NormalizeUrlsRecursively(contentElement, "https://www.government.bg/");
            var content = contentElement.InnerHtml.Trim();

            var news = new RemoteNews
                       {
                           OriginalUrl = url,
                           RemoteId = this.ExtractIdFromUrl(url),
                           Title = title,
                           Content = content,
                           PostDate = time,
                           ImageUrl = imageUrl,
                       };
            return news;
        }

        internal string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                         ? uri.Segments[uri.Segments.Length - 1].Trim('/')
                         : uri.Segments[uri.Segments.Length - 2].Trim('/');
            return id;
        }
    }
}
