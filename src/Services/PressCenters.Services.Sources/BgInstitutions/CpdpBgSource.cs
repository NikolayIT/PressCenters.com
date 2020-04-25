namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Комисия за защита на личните данни.
    /// </summary>
    public class CpdpBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.cpdp.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("/", ".center-part h6 a.news-title");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i < 1520; i++)
            {
                var news = this.GetPublication($"https://www.cpdp.bg/index.php?p=news_view&aid={i}");
                if (news != null)
                {
                    yield return news;
                    Console.WriteLine($"{i} => {news.Title}");
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => this.GetUrlParameterValue(url, "aid");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            if (document.QuerySelector(".center-part .path")?.TextContent?.Contains("» По жалби") == true)
            {
                return null;
            }

            var titleElement = document.QuerySelector(".center-part h1");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();
            if (title == "Добре дошли!")
            {
                return null;
            }

            var timeElement = document.QuerySelector(".center-part .date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".center-part .titleImage img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".center-part");
            contentElement.RemoveRecursively(document.QuerySelector(".center-part .path"));
            contentElement.RemoveRecursively(titleElement);
            contentElement.RemoveRecursively(timeElement);
            contentElement.RemoveRecursively(document.QuerySelector(".center-part .titleImage"));
            contentElement.RemoveRecursively(document.QuerySelector(".center-part #content-bottom"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
