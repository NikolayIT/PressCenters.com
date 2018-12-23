namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;

    public class ToploBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            var address = "https://toplo.bg/news";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".post a")
                .Select(x => this.NormalizeUrl(x.Attributes["href"]?.Value, "https://toplo.bg/")).ToList();

            var news = links.Select(this.ParseRemoteNews).ToList();
            return new RemoteDataResult { News = news, };
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".l9 .card-title strong");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".l9 .post_author_date .post_content_date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM, yyyy", CultureInfo.GetCultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".l9 .card-content .card-content");
            this.NormalizeUrlsRecursively(contentElement, "https://toplo.bg/");
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".l9 .card-image img.img-blog");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "https://toplo.bg/")?.Trim();

            var news = new RemoteNews
                           {
                               OriginalUrl = url,
                               RemoteId = this.ExtractIdFromUrl(url),
                               Title = title,
                               Content = content,
                               PostDate = time,
                               ShortContent = null,
                               ImageUrl = imageUrl,
                           };
            return news;
        }

        internal string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim('/'));
            return uri.Segments[uri.Segments.Length - 4] +
                   uri.Segments[uri.Segments.Length - 3] +
                   uri.Segments[uri.Segments.Length - 2] +
                   uri.Segments[uri.Segments.Length - 1];
        }
    }
}
