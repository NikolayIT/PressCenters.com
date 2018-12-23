namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Extensions;

    using PressCenters.Common;

    public class FscBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            var address = "http://www.fsc.bg/bg/novini/";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links =
                document.QuerySelectorAll(".news-box-listing a")
                    .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, "http://www.fsc.bg/"))
                    .Where(x => this.ExtractIdFromUrl(x).ToInteger() > localInfo.LastLocalId.ToInteger())
                    .ToList();

            var news = links.Select(this.ParseRemoteNews).ToList();
            return new RemoteDataResult { News = news, };
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var titleElement = document.QuerySelector("#content-left-inner h2");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector("#content-left-inner .article_date");
            var time = DateTime.ParseExact(timeElement.TextContent, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            time = time.Date == DateTime.Now.Date ? DateTime.Now : time.AddHours(12);

            var contentElement = document.QuerySelector("#content-left-inner");
            contentElement.RemoveElement(titleElement);
            contentElement.RemoveElement(timeElement);

            var imageUrl = document.QuerySelector("#content-right img")?.Attributes?["src"]?.Value;

            var news = new RemoteNews
            {
                Title = title,
                OriginalUrl = url,
                RemoteId = this.ExtractIdFromUrl(url),
                PostDate = time,
                ShortContent = null,
                Content = contentElement.InnerHtml.Trim(),
                ImageUrl = this.NormalizeUrl(imageUrl, "http://www.fsc.bg/"),
            };
            return news;
        }

        internal string ExtractIdFromUrl(string url)
        {
            const string EndString = ".html";
            if (string.IsNullOrWhiteSpace(url))
            {
                return "0";
            }

            int startIndex = url.LastIndexOf("-", StringComparison.Ordinal);
            if (startIndex == -1)
            {
                return string.Empty;
            }

            int endIndex = url.LastIndexOf(EndString, StringComparison.Ordinal);
            if (endIndex == -1)
            {
                return string.Empty;
            }

            return url.Substring(startIndex + 1, url.Length - EndString.Length - startIndex - 1);
        }
    }
}
