namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Extensions;

    public class PrbBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            var address = "https://www.prb.bg/bg/news/aktualno";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links =
                document.QuerySelectorAll(".list-field .list-content a")
                    .Select(x => x.Attributes?["href"]?.Value)
                    .Select(x => this.NormalizeUrl(x, "https://www.prb.bg/"))
                    .ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            var remoteDataResult = new RemoteDataResult { News = news, LastNewsIdentifier = string.Empty };
            return remoteDataResult;
        }

        internal RemoteNews ParseRemoteNews(string url)
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
            this.NormalizeUrlsRecursively(contentElement, "https://www.prb.bg/");
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".slide img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "https://www.prb.bg/").Trim();

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
            var uri = new Uri(url);
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                         ? uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1].Trim('/')
                         : uri.Segments[uri.Segments.Length - 3] + uri.Segments[uri.Segments.Length - 2].Trim('/');
            return id;
        }
    }
}
