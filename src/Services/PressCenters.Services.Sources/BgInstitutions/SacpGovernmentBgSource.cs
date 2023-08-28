namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Държавна агенция за закрила на детето.
    /// </summary>
    public class SacpGovernmentBgSource : BaseSource
    {
        public override string BaseUrl => "https://sacp.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("новини", ".view-content a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 0; i <= 92; i++)
            {
                var news = this.GetPublications($"новини?page={i}", "h2 a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.node-title");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".node-date");
            timeElement.RemoveRecursively(document.QuerySelector(".node-date ul"));
            var timeAsString = timeElement.TextContent.Replace("г.", string.Empty).Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".node-image img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".node-body");
            contentElement.RemoveRecursively(timeElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
