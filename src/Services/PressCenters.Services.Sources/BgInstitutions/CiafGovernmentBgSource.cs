namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Комисия за противодействие на корупцията и за отнемане на незаконно придобитото имущество.
    /// </summary>
    public class CiafGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.caciaf.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("aktualno/novini", "article.article a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 46; i++)
            {
                var news = this.GetPublications($"aktualno/novini?page={i}", "article.article a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("article.inner-block h2");
            if (titleElement == null)
            {
                return null;
            }

            var title = new CultureInfo("bg-BG", false).TextInfo.ToTitleCase(
                titleElement.TextContent?.Trim()?.ToLower() ?? string.Empty);

            var timeElement = document.QuerySelector("article.inner-block time.inner-block__date");
            var timeAsString = timeElement.Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);

            var contentElement = document.QuerySelector("article.inner-block div.text");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector("article.inner-block img");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
