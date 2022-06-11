namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    public class CemBgSource : BaseSource
    {
        public override string BaseUrl => "https://www.cem.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("newsbg", ".boxNews a", count: 5, urlShouldContain: "displaynewsbg");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 34; page++)
            {
                var news = this.GetPublications($"newsbg/page-{page}", ".boxNews a", urlShouldContain: "displaynewsbg");
                Console.WriteLine($"Page {page} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".articleEntry h3");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var contentElement = document.QuerySelector(".articleEntryDescription");

            var imageElement = contentElement.QuerySelector("img");
            var imageUrl = imageElement?.GetAttribute("src");
            if (imageUrl?.EndsWith("images/file.png") == true)
            {
                imageElement = null;
                imageUrl = null;
            }

            var timeElement = contentElement.QuerySelector(".date");
            var time = DateTime.ParseExact(timeElement?.TextContent.ToLower().Trim(), "dd MMMM yyyy", CultureInfo.GetCultureInfo("bg-BG"));

            contentElement.RemoveRecursively(imageElement);
            contentElement.RemoveRecursively(timeElement);
            contentElement.RemoveRecursively(contentElement.QuerySelector("script"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
