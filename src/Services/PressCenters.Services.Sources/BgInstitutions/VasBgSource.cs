namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    internal class VasBgSource : BaseSource
    {
        public override string BaseUrl => "https://www.vas.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
            => this.GetPublications("bg/c/news", ".itemscontainer .card-title a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 45; i++)
            {
                var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}bg/c/news?page={i}"));
                var newsElements = document.QuerySelectorAll(".itemscontainer .mb-5 .col-md-8");
                var newsCount = 0;
                foreach (var newsElement in newsElements)
                {
                    var url = this.NormalizeUrl(newsElement.QuerySelector(".card-title a").Attributes["href"].Value);
                    var dateAsString = newsElement.QuerySelector(".card-text small").TextContent.Trim();
                    var date = dateAsString.StartsWith("Днес") ? DateTime.Now.Date : dateAsString.StartsWith("Вчера") ? DateTime.Now.AddDays(-1).Date
                        : DateTime.ParseExact(dateAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
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
            var titleElement = document.QuerySelector("h1");
            var title = titleElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var contentElement = document.QuerySelector("article");

            var imageElement = contentElement.QuerySelector("img");
            var imageUrl = imageElement?.GetAttribute("src");

            contentElement.RemoveRecursively(imageElement);
            contentElement.RemoveRecursively(contentElement.QuerySelector("h1"));
            contentElement.RemoveRecursively(contentElement.QuerySelector("hr"));
            contentElement.RemoveRecursively(contentElement.QuerySelector(".date"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, DateTime.Now, imageUrl);
        }
    }
}
