namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    /// <summary>
    /// Омбудсман на Република България.
    /// </summary>
    public class OmbudsmanBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.ombudsman.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/p/novini", ".m-title a", count: 5);

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".m-article h2");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".m-article .text-muted");
            var timeAsString = timeElement.TextContent.Split(",")[1].Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".m-article .m-news-image");
            var imageUrl = imageElement?.Attributes?["src"]?.Value;

            var contentElement = document.QuerySelector(".m-article");
            contentElement.RemoveRecursively(titleElement);
            contentElement.RemoveRecursively(timeElement);
            contentElement.RemoveRecursively(document.QuerySelector(".m-article a"));
            contentElement.RemoveRecursively(document.QuerySelector(".m-article .justify-content-center"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
