namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Държавна агенция за метрологичен и технически надзор (ДАМТН).
    /// </summary>
    public class DamtnGovernmentBgSource : BaseSource
    {
        public override string BaseUrl => "https://www.damtn.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("category/novini/", ".entry-title a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 52; i++)
            {
                var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}category/novini/page/{i}/"));
                var newsElements = document.QuerySelectorAll(".grid-box");
                var newsCount = 0;
                foreach (var newsElement in newsElements)
                {
                    var urlElement = newsElement.QuerySelector(".entry-title a");
                    var url = this.NormalizeUrl(urlElement.Attributes["href"].Value);
                    var dateAsString = newsElement.QuerySelector(".meta-date").TextContent.Trim();
                    var date = DateTime.ParseExact(dateAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    var remoteNews = this.GetPublication(url);
                    if (remoteNews == null)
                    {
                        continue;
                    }

                    newsCount++;
                    remoteNews.PostDate = date;
                    yield return remoteNews;
                }

                Console.WriteLine($"Page {i} => {newsCount} news");
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.page-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var contentElement = document.QuerySelector(".entry-content");

            var imageElement = document.QuerySelector(".entry-content img");
            var imageUrl = imageElement?.GetAttribute("src");

            var timeElement = document.QuerySelector(".updated");
            var time = DateTime.Parse(timeElement?.TextContent);

            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
