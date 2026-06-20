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

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetNews(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow.AddDays(1), 6);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            // Query month-by-month: a full-year rPP=10000 query is slow enough to hit the HttpClient
            // timeout through the relay. 2015..2023 is already fully captured, so start at 2024 (the
            // scraper-outage gap) and walk forward to the current month.
            var end = DateTime.UtcNow.AddMonths(1);
            for (var month = new DateTime(2024, 1, 1); month < end; month = month.AddMonths(1))
            {
                Console.WriteLine($"{month:yyyy-MM}...");
                foreach (var remoteNews in this.GetNews(month, month.AddMonths(1), 1000))
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[^2] + uri.Segments[^1];
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".content-inner-wrapper h2");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector("#publish-date");
            var timeAsString = timeElement?.TextContent?.Replace("Дата:", string.Empty)?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".galleryList img");
            var imageUrl = imageElement?.GetAttribute("src");
            imageUrl = imageUrl?.Replace("/myconnect/", "/connect/");

            var contentElement = document.QuerySelector("div.vp-news-text");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }

        // Lazy: yield each article as it is fetched, so a backfill's per-item throttle and per-article
        // resilience apply. The eager version fetched a whole year up front, bursting the relay.
        private IEnumerable<RemoteNews> GetNews(DateTime from, DateTime to, int results)
        {
            var url =
                $"{this.BaseUrl}customSearchWCM/query?context=customs.bg28892&libName=agency&saId={this.SaId}&atId=72bd8711-7a20-4b13-85dd-bb04e5c1102f&filterByElements=&rootPage=agency&rPP={results}&currentPage=1&currentUrl=https%3A%2F%2Fcustoms.bg%2F{this.NewsUrl}&dateFormat=dd.MM.yyyy&ancestors=false&descendants=true&orderBy=publishDate&orderBy2=publishDate&orderBy3=title&sortOrder=false&searchTerm=&useQuery=true&from={from:dd.MM.yyyy}&before={to:dd.MM.yyyy}";
            var json = this.ReadStringFromUrl(url);
            var newsAsJson = JsonConvert.DeserializeObject<IEnumerable<NewsAsJson>>(json);
            foreach (var item in newsAsJson)
            {
                if (item?.ContentUrl?.Url == null)
                {
                    continue;
                }

                var publication = this.GetPublication(item.ContentUrl.Url);
                if (publication != null)
                {
                    yield return publication;
                }
            }
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
