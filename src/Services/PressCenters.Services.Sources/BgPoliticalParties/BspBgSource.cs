namespace PressCenters.Services.Sources.BgPoliticalParties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp;

    using PressCenters.Common;

    public class BspBgSource : BaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = "http://bsp.bg/news.html";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".post-content h3 a").Select(x => x.Attributes["href"].Value)
                .Select(x => this.NormalizeUrl(x, "http://bsp.bg")).ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            return news;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".post-content h2");
            var title = titleElement.TextContent.Trim();

            var bulgarianMonthNames = new List<string>
                                          {
                                              "януари", "февруари", "март", "април", "май", "юни", "юли", "август",
                                              "септември", "октомври", "ноември", "декември",
                                          };
            var dateAsString = document.QuerySelector(".meta_date").InnerHtml.Trim();
            var monthName = dateAsString.Substring(0, 3).ToLower();
            var monthIndex = bulgarianMonthNames.FindIndex(x => x.StartsWith(monthName)) + 1;
            if (monthIndex == 0)
            {
                monthIndex = DateTime.UtcNow.Month;
            }

            var dayOfMonth = dateAsString.Substring(4, 2).ToInteger();
            var year = dateAsString.Substring(dateAsString.Length - 4, 4).ToInteger();
            var time = new DateTime(year, monthIndex, dayOfMonth, 8, 0, 0);
            if (time.Date == DateTime.Now.Date)
            {
                time = DateTime.Now;
            }

            var contentElement = document.QuerySelector(".post-content");
            this.NormalizeUrlsRecursively(contentElement, "http://bsp.bg/");
            this.RemoveRecursively(contentElement, document.QuerySelector(".post-content h2"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".post-content .social-buttons"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".post-content .item_meta"));
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".post-poster img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"), "http://bsp.bg/").Trim();

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
            var id = url.GetStringBetween("news/view/", "-");
            return id;
        }
    }
}
