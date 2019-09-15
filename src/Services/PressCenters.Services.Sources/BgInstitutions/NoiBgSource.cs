namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    using PressCenters.Common;

    /// <summary>
    /// Национален осигурителен институт.
    /// </summary>
    public class NoiBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.noi.bg/";

        protected override List<(HttpRequestHeader header, string value)> Headers =>
            new List<(HttpRequestHeader header, string value)>
            {
                (HttpRequestHeader.Cookie, "cb379c5a647843b32707848c89ea970d=bg-BG"),
            };

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("newsbg", "h2.nssi-postheader a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var year = 2011; year <= 2017; year++)
            {
                for (var page = 1; page <= 15; page++)
                {
                    var remoteNews = this.GetPublications(
                        $"newsbg/{year}?start={(page - 1) * 5}",
                        "h2.nssi-postheader a");
                    Console.WriteLine($"{year} / {page} => {remoteNews.Count}");
                    foreach (var news in remoteNews)
                    {
                        yield return news;
                    }
                }
            }

            for (var page = 1; page <= 2; page++)
            {
                var remoteNews = this.GetPublications(
                    $"newsbg?start={(page - 1) * 40}",
                    "h2.nssi-postheader a");
                Console.WriteLine($"Page {page} => {remoteNews.Count}");
                foreach (var news in remoteNews)
                {
                    yield return news;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => url.GetLastStringBetween("/", "-");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h2.nssi-postheader");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector(".nssi-postdateicon");
            var timeAsString = timeElement?.TextContent?.Trim().ToLower();
            DateTime time;
            if (timeAsString == "сряда, 30 ноември -0001 02:00")
            {
                var regex = new Regex(@"d{4}");
                var match = regex.Match(title);
                var year = match.Groups[0].ToString();
                time = new DateTime(year != string.Empty ? year.ToInteger() : 2013, 12, 1);
            }
            else
            {
                time = DateTime.ParseExact(timeAsString, "dddd, dd MMMM yyyy HH:mm", new CultureInfo("bg-BG"));
            }

            var imageElement = document.QuerySelector(".nssi-article img[src*='News']");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/noi.bg.jpg";

            var contentElement = document.QuerySelector(".nssi-article");
            contentElement.RemoveRecursively(imageElement);
            contentElement.RemoveRecursively(contentElement.QuerySelector("ul.pagenav"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
