namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на труда и социалната политика.
    /// </summary>
    //// TODO: Rename to MlspGovernmentBgSource
    public class MlspBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mlsp.government.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("novini", ".post__widget .post__title a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 10; page++)
            {
                var news = this.GetPublications($"novini?page={page}", ".page-content .post__title a");
                Console.WriteLine($"Page {page} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h3.post__title");
            var title = new CultureInfo("bg-BG", false).TextInfo.ToTitleCase(
                titleElement?.TextContent.ToLower() ?? string.Empty);

            var timeAsString = document.QuerySelector(".post__created-at")?.TextContent.Trim();
            var time = DateTime.Parse(timeAsString, CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".post__content img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".post__content");
            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
