namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Extensions;

    public class FscBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.fsc.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}bg/novini/";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".news-box-listing a")
                .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl)).ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override string ExtractIdFromUrl(string url)
        {
            const string EndString = ".html";
            if (string.IsNullOrWhiteSpace(url))
            {
                return "0";
            }

            var startIndex = url.LastIndexOf("-", StringComparison.Ordinal);
            if (startIndex == -1)
            {
                return string.Empty;
            }

            var endIndex = url.LastIndexOf(EndString, StringComparison.Ordinal);
            if (endIndex == -1)
            {
                return string.Empty;
            }

            return url.Substring(startIndex + 1, url.Length - EndString.Length - startIndex - 1);
        }

        protected override RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector("#content-left-inner h2");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector("#content-left-inner .article_date");
            var time = DateTime.ParseExact(timeElement.TextContent, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            time = time.Date == DateTime.UtcNow.Date ? DateTime.Now : time;

            var imageUrl = document.QuerySelector("#content-right img")?.Attributes?["src"]?.Value;

            var contentElement = document.QuerySelector("#content-left-inner");
            contentElement.RemoveElement(titleElement);
            contentElement.RemoveElement(timeElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
