namespace PressCenters.Sources.BgInstitutions
{
    using System;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;

    public class ApiBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            var address = "http://www.api.bg/index.php/tools/blocks/news_list/rss?bID=606&cID=186&arHandle=Main";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll("item > link").Select(x => x.InnerHtml).ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            var remoteDataResult = new RemoteDataResult { News = news, LastNewsIdentifier = string.Empty };
            return remoteDataResult;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var title = document.QuerySelector(".box h1").TextContent?.Trim();
            var contentElement = document.QuerySelector(".news-article");

            var timeNode = contentElement.ChildNodes[0];
            var time = DateTime.ParseExact(timeNode.TextContent, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture); // 24.01.2016 09:59

            var imageElement = document.QuerySelector(".news-article img");
            this.RemoveRecursively(contentElement, imageElement);
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "http://www.api.bg/").Trim();

            contentElement.RemoveChild(timeNode);
            var remoteNews = new RemoteNews
                                 {
                                     OriginalUrl = url,
                                     Title = title,
                                     ImageUrl = imageUrl,
                                     Content = contentElement.InnerHtml,
                                     PostDate = time,
                                     RemoteId = this.ExtractIdFromUrl(url),
                                 };
            return remoteNews;
        }

        internal string ExtractIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            var uri = new Uri(url);
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                       ? uri.Segments[uri.Segments.Length - 1]
                       : uri.Segments[uri.Segments.Length - 2];
            id = id.Replace("/", string.Empty);
            return id;
        }
    }
}
