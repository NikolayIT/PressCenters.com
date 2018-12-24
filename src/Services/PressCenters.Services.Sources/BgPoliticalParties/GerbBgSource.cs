namespace PressCenters.Services.Sources.BgPoliticalParties
{
    using System;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;

    using PressCenters.Common;

    public class GerbBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications()
        {
            var address = "http://gerb.bg/bg/news/spisyk-novini-1.html";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll("#container-main-ajax article p a")
                .Select(x => x.Attributes["href"].Value).Select(x => this.NormalizeUrl(x, "http://gerb.bg")).ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            return new RemoteDataResult { News = news, };
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".news-info h1");
            var title = titleElement.TextContent.Trim();

            var dateAsString = document.QuerySelector(".news-info .date").InnerHtml;
            var time = DateTime.ParseExact(dateAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            time = time.Date == DateTime.Now.Date ? DateTime.Now : time.AddHours(8);

            var contentElement = document.QuerySelector(".news-info");
            this.NormalizeUrlsRecursively(contentElement, "http://gerb.bg/");
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info .social"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info .date"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info h1"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info .more"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".news-info .social-r"));
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector("#slider img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "http://gerb.bg/")?.Trim();

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
            var lastDash = url.LastIndexOf("-", StringComparison.InvariantCulture);
            var id = url.GetStringBetween("-", ".html", lastDash);
            return id;
        }
    }
}
