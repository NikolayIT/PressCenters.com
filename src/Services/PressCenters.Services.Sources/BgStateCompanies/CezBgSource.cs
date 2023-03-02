namespace PressCenters.Services.Sources.BgStateCompanies
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// „ЧЕЗ България” ЕАД.
    /// </summary>
    public class CezBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://electrohold.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/mediya-centr-group/novini/", "a.card-content__button", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 19; page++)
            {
                var news = this.GetPublications($"bg/mediya-centr-group/novini/?page={page}", "a.card-content__button");
                Console.WriteLine($"Page {page} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".detail-header__title");
            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".detail-header__date");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy", new CultureInfo("bg-BG"));

            var imageElement = document.QuerySelector("source.present-header__image");
            var imageUrl = imageElement?.GetAttribute("srcset");

            var contentElement = document.QuerySelector(".richtext");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
