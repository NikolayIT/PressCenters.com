namespace PressCenters.Sources.BgInstitutions
{
    using System;
    using System.Linq;

    using AngleSharp;

    public class PrbBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            var address = "http://www.prb.bg/bg/news/aktualno/";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links =
                document.QuerySelectorAll(".list-group-item > h3 > a")
                    .Select(x => x.Attributes?["href"]?.Value)
                    .Select(x => this.NormalizeUrl(x, "http://www.prb.bg/"))
                    .ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            var remoteDataResult = new RemoteDataResult { News = news, LastNewsIdentifier = string.Empty };
            return remoteDataResult;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".article h1");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".article time");
            var timeAsString = timeElement.Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);

            var contentElement = document.QuerySelector(".article");
            this.RemoveRecursively(contentElement, titleElement);
            this.RemoveRecursively(contentElement, timeElement);
            this.RemoveRecursively(contentElement, document.QuerySelector(".tab-container"));
            this.NormalizeUrlsRecursively(contentElement, "http://www.prb.bg/");
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".article img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "http://www.prb.bg/").Trim();

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
