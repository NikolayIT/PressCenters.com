namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;

    using PressCenters.Common;

    public class PresidentBgSource : BaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = "https://www.president.bg/news/";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".inside-article-box > a").Select(
                x => this.NormalizeUrl(x.Attributes["href"].Value, "http://www.president.bg/")).ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            return news;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".print-content h2");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".print-content .date");
            var timeAsString = timeElement.TextContent;
            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy | HH:mm", CultureInfo.GetCultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".print-content .index-news-bdy");
            this.NormalizeUrlsRecursively(contentElement, "http://www.president.bg/");
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".print-content img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "http://www.president.bg/").Trim();

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

        internal string ExtractIdFromUrl(string originalUrl)
        {
            if (originalUrl == null)
            {
                return null;
            }

            if (originalUrl.StartsWith("https"))
            {
                return originalUrl.GetStringBetween("https://www.president.bg/news", "/");
            }
            else
            {
                return originalUrl.GetStringBetween("http://www.president.bg/news", "/");
            }
        }
    }
}
