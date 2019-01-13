namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    using Newtonsoft.Json;

    /// <summary>
    /// Агенция "Митници".
    /// </summary>
    public abstract class CustomsBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://customs.bg/";

        public abstract string NewsUrl { get; }

        public abstract string SaId { get; }

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetNews(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow.AddDays(1), 6);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var year = 2015; year <= 2019; year++)
            {
                var news = this.GetNews(new DateTime(year, 1, 1), new DateTime(year + 1, 1, 1), 10000);
                Console.WriteLine($"Year {year} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1];
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector("div.vp-section-main-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector("#publish-date");
            var timeAsString = timeElement?.TextContent?.Replace("Дата:", string.Empty)?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy", new CultureInfo("en-US"));

            var imageElement = document.QuerySelector(".galleryList img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/customs.bg.jpg";

            var contentElement = document.QuerySelector("div.vp-news-text");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }

        private IList<RemoteNews> GetNews(DateTime from, DateTime to, int results)
        {
            var url =
                $"{this.BaseUrl}customSearchWCM/customsearch?context=customs.bg28892&libName=agency&saId={this.SaId}&atId=72bd8711-7a20-4b13-85dd-bb04e5c1102f&filterByElements=&rootPage=agency&rPP={results}&currentPage=1&currentUrl=https%3A%2F%2Fcustoms.bg%2F{this.NewsUrl}&dateFormat=dd.MM.yyyy&ancestors=false&descendants=true&orderBy=publishDate&orderBy2=publishDate&orderBy3=title&sortOrder=false&searchTerm=&useQuery=true&from={from:dd.MM.yyyy}&before={to:dd.MM.yyyy}";
            var json = this.ReadStringFromUrl(url);
            var newsAsJson = JsonConvert.DeserializeObject<IEnumerable<NewsAsJson>>(json);
            var news = newsAsJson.Select(
                x => x?.ContentUrl?.Url == null ? null : this.GetPublication(x.ContentUrl.Url)).Where(x => x != null);
            return news.ToList();
        }

        private class NewsAsJson
        {
            public ContentUrl ContentUrl { get; set; }
        }

        private class ContentUrl
        {
            public string Url { get; set; }
        }
    }
}
