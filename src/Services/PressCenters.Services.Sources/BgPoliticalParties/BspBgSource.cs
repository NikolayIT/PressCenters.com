namespace PressCenters.Sources.BgPoliticalParties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp;

    using PressCenters.Common;

    public class BspBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            // TODO: Extract this code (same as GERB and few other sources)
            var address = "http://bsp.bg/news.html";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links =
                document.QuerySelectorAll(".post-content h3 a")
                    .Select(x => x.Attributes["href"].Value)
                    .Select(x => this.NormalizeUrl(x, "http://bsp.bg"))
                    .Where(x => this.ExtractIdFromUrl(x).ToInteger() > localInfo.LastLocalId.ToInteger())
                    .ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();

            var lastIdentifier = localInfo.LastLocalId;
            if (news.Any())
            {
                lastIdentifier =
                    this.ExtractIdFromUrl(news.OrderByDescending(x => x.PostDate).FirstOrDefault()?.OriginalUrl);
            }

            var remoteDataResult = new RemoteDataResult { News = news, LastNewsIdentifier = lastIdentifier, };
            return remoteDataResult;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector(".post-content h2");
            var title = titleElement.TextContent.Trim();

            var shortContentElement = document.QuerySelector(".post-content h4");
            var shortContent = shortContentElement.TextContent.Trim();

            // TODO: Extract month names (same as toplo.bg source)
            var bulgarianMonthNames = new List<string>
                                          {
                                              "януари", "февруари", "март", "април", "май", "юни", "юли", "август",
                                              "септември", "октомври", "ноември", "декември",
                                          };
            var dateAsString = document.QuerySelector(".meta_date").InnerHtml.Trim();
            var monthName = dateAsString.Substring(0, 3).ToLower();
            var monthIndex = bulgarianMonthNames.FindIndex(x => x.StartsWith(monthName)) + 1;
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
            this.RemoveRecursively(contentElement, document.QuerySelector(".post-content h4"));
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
                               ShortContent = shortContent,
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
