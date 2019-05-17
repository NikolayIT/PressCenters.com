namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    using PressCenters.Common;

    /// <summary>
    /// Национална агенция за приходите.
    /// </summary>
    public class NapBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.nap.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}page?id=223"));
            var links = document.QuerySelectorAll(".news_list li a").Select(x => x?.Attributes["onclick"]?.Value)
                .Where(x => x?.Contains("ShowWindow('/news?id=") == true).Select(
                    x => $"{this.BaseUrl}{x.GetStringBetween("ShowWindow('/", "'")}").ToList();
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 3850; i++)
            {
                var remoteNews = this.GetPublication($"{this.BaseUrl}news?id={i}");
                if (remoteNews == null)
                {
                    continue;
                }

                Console.WriteLine($"№{i} => {remoteNews.PostDate.ToShortDateString()} => {remoteNews.Title}");
                yield return remoteNews;
            }
        }

        internal override string ExtractIdFromUrl(string url) => this.GetUrlParameterValue(url, "id");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector(".date");
            if (timeElement == null)
            {
                return null;
            }

            var timeAsString = timeElement?.TextContent?.Trim().ToLower();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy", new CultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".content");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            if (content?.Length < 50 || content.Contains("Р”Р°РЅСЉС†Рё"))
            {
                return null;
            }

            return new RemoteNews(title, content, time, "/images/sources/nap.bg.jpg");
        }
    }
}
