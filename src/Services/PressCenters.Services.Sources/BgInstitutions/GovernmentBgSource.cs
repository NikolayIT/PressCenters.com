namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using AngleSharp;

    using PressCenters.Common;

    public class GovernmentBgSource : BaseSource
    {
        private const string DefaultImageUrl = "http://www.government.bg/fce/001/tmpl/bigimg/gerb.jpg";

        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            var address = "http://www.government.bg/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&g=";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links =
                document.QuerySelectorAll("a[href^='/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&n=']")
                    .Take(25)
                    .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, "http://www.government.bg/"))
                    .Where(x => this.ExtractIdFromUrl(x).ToInteger() > localInfo.LastLocalId.ToInteger())
                    .ToList();

            var news = links.Select(this.ParseRemoteNews).ToList();
            var remoteDataResult = new RemoteDataResult
                                       {
                                           News = news,
                                           LastNewsIdentifier =
                                               this.ExtractIdFromUrl(
                                                   news.OrderByDescending(x => x.RemoteId.ToInteger())
                                                       .FirstOrDefault()?.OriginalUrl),
                                       };
            return remoteDataResult;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;

            // Awful selectors, awful site...
            // [0] => Title
            // [1] => Date
            // [2] => Content
            var mainNewsElements =
                document.QuerySelectorAll(
                    "body > table > tbody > tr:nth-child(2) > td > table:nth-child(3) > tbody > tr > td:nth-child(3) > table > tbody > tr:nth-child(2) > td > table > tbody > tr > td");
            var title = mainNewsElements[0].TextContent.Trim();
            var time = DateTime.ParseExact(
                mainNewsElements[1]?.TextContent?.Trim(),
                "dd MMMM yyyy",
                CultureInfo.GetCultureInfo("bg-BG"),
                DateTimeStyles.AllowWhiteSpaces);
            time = time.Date == DateTime.Now.Date ? DateTime.Now : time.AddHours(12);

            var contentElement = mainNewsElements[2];
            this.NormalizeUrlsRecursively(contentElement, "http://www.government.bg/");
            var imageElement = mainNewsElements[2].QuerySelector("img");
            var imageUrl = imageElement?.Attributes?["src"]?.Value ?? DefaultImageUrl;
            if (imageElement != null)
            {
                this.RemoveRecursively(contentElement, imageElement.Parent);
            }

            var news = new RemoteNews
                           {
                               Title = title,
                               OriginalUrl = url,
                               RemoteId = this.ExtractIdFromUrl(url),
                               PostDate = time,
                               ShortContent = null,
                               Content = contentElement.InnerHtml.Trim(),
                               ImageUrl = this.NormalizeUrl(imageUrl, "http://www.government.bg/"),
                           };
            return news;
        }

        internal string ExtractIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "0";
            }

            var queryString = url.Substring(url.IndexOf('?')).Split('#')[0];
            var parameters = HttpUtility.ParseQueryString(queryString);
            var id = parameters["n"] ?? string.Empty;
            return id;
        }
    }
}
