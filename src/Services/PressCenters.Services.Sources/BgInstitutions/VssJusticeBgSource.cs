namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using AngleSharp.Dom;

    public class VssJusticeBgSource : BaseSource
    {
        public override string BaseUrl => "http://www.vss.justice.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
            => this.GetPublications("page/view/2574", ".right_info .row a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 365; i++)
            {
                var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}page/view/2574?p={i}"));
                var newsElements = document.QuerySelectorAll(".right_info .row");
                var newsCount = 0;
                foreach (var newsElement in newsElements)
                {
                    var url = this.NormalizeUrl(newsElement.QuerySelector("a").Attributes["href"].Value);
                    var dateAsString = newsElement.QuerySelector(".new_date").TextContent.Trim();
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
                Thread.Sleep(1500);
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".right_info .title");
            var title = titleElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Прессъобщение";
            }

            var contentElement = document.QuerySelector(".right_info .with_left_menu_content");

            var imageElement = contentElement.QuerySelector("img");
            var imageUrl = imageElement?.GetAttribute("src");
            contentElement.RemoveRecursively(imageElement);

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, DateTime.Now, imageUrl);
        }
    }
}
