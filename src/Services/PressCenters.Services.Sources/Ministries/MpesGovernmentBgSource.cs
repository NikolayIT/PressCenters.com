namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на младежта и спорта.
    /// </summary>
    public class MpesGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.mpes.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(
                "Pages/Press/default.aspx",
                "#DivEventList .RadGridItem h3 a",
                "Pages/Press/News/Default.aspx");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 4105; i++)
            {
                var remoteNews = this.GetPublication($"{this.BaseUrl}Pages/Press/News/Default.aspx?evntid={i}");
                if (remoteNews == null)
                {
                    continue;
                }

                Console.WriteLine($"News {i} => {remoteNews.PostDate.ToShortDateString()} => {remoteNews.Title}");
                yield return remoteNews;
            }
        }

        public override string ExtractIdFromUrl(string url) => this.GetUrlParameterValue(url, "evntid");

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector(".PanelFullText h1");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var imageElement = document.QuerySelector(".PanelFullText .event_text table img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/mpes.government.bg.jpg";

            var contentElement = document.QuerySelector(".PanelFullText .event_text");
            contentElement.RemoveRecursively(document.QuerySelector(".PanelFullText .event_text table:has(img)"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;
            if (string.IsNullOrWhiteSpace(content?.Replace("<br>", string.Empty).Trim()))
            {
                return null;
            }

            var timeElement = document.QuerySelector(".PanelFullText");
            timeElement.RemoveRecursively(document.QuerySelector(".PanelFullText .event_text"));
            timeElement.RemoveRecursively(document.QuerySelector(".PanelFullText h1"));
            timeElement.RemoveRecursively(document.QuerySelector(".PanelFullText h5"));
            timeElement.RemoveRecursively(document.QuerySelector(".PanelFullText h5")); // Sometimes there are 2 h5 elements
            var timeAsString = timeElement?.TextContent?.Trim();
            if (timeAsString?.Length < 10)
            {
                return null;
            }

            timeAsString = timeAsString?.Substring(0, 10);
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
