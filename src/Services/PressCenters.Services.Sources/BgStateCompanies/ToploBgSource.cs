namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using AngleSharp;

    using PressCenters.Common;

    public class ToploBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            var address = "http://toplo.bg/all-news/";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links =
                document.QuerySelectorAll(".Desktop .PageElementsMargin .RowsContainer .DataContainer .Button")
                    .Select(x => x.Attributes["href"].Value)
                    .Select(x => this.NormalizeUrl(x, "http://toplo.bg"))
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
            var titleElement = document.QuerySelector(".Title .Table .Cell");
            var title = titleElement.TextContent.Trim();

            var bulgarianMonthNames = new List<string>
                                          {
                                              "януари", "февруари", "март", "април", "май", "юни", "юли", "август",
                                              "септември", "октомври", "ноември", "декември",
                                          };
            var dayOfMonth = document.QuerySelector(".Title .Info .Value").InnerHtml.ToInteger();
            var monthName = document.QuerySelector(".Title .Info .Desc").InnerHtml;
            var monthIndex = bulgarianMonthNames.FindIndex(x => x.StartsWith(monthName)) + 1;
            var time = new DateTime(DateTime.Now.Year, monthIndex, dayOfMonth, 8, 0, 0);
            if (time.Date == DateTime.Now.Date)
            {
                time = DateTime.Now;
            }

            var contentElement = document.QuerySelector(".RowEntry .Data");
            this.NormalizeUrlsRecursively(contentElement, "http://toplo.bg/");
            var content = contentElement.InnerHtml.Trim();

            // toplo.bg have no images in their news
            var imageUrl = "/Content/Logos/toplo.bg.png";

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
            if (url == null)
            {
                return null;
            }

            var uri = new Uri(url);
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            return parameters["id"];
        }
    }
}
