namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;

    using AngleSharp;

    public class MvrBgSource : BaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = "https://www.mvr.bg/press/актуална-информация/актуална-информация/актуално";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".article__list .article .article__description a")
                .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, "https://www.mvr.bg/")).Distinct().ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            return news;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".article__description h1");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".article__description h5");
            var timeAsString = timeElement?.TextContent?.Trim();
            if (!DateTime.TryParseExact(timeAsString, "dd MMM yyyy", CultureInfo.GetCultureInfo("bg-BG"), DateTimeStyles.None, out DateTime time))
            {
                timeElement = document.QuerySelector(".article__description .timestamp");
                timeAsString = timeElement?.TextContent?.Trim();
                time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy", CultureInfo.GetCultureInfo("bg-BG"));
            }

            var imageElement = document.QuerySelector("#image_source");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "https://www.mvr.bg/")?.Trim();

            var contentElement = document.QuerySelector(".article__container");
            this.RemoveRecursively(contentElement, document.QuerySelector(".article__container div.row"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".article__container script"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".article__container .pull-right"));
            this.NormalizeUrlsRecursively(contentElement, "https://www.mvr.bg/");
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
            return WebUtility.UrlDecode(id);
        }
    }
}
