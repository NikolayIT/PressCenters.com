namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    public class BasBgSource : BaseSource
    {
        public override string BaseUrl => "https://www.bas.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(string.Empty, ".fusion-recent-posts article.post h4 a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 36; page++)
            {
                var news = this.GetPublications(
                    $"академични-новини/page/{page}",
                    ".fusion-recent-posts article.post h4 a");
                Console.WriteLine($"Page {page} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => this.GetUrlParameterValue(url, "p");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.fusion-post-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".updated.rich-snippet-hidden").NextElementSibling;
            var timeAsString = timeElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            // bas.bg has emitted this date in several shapes over the years: "понеделник, 9 май 2017"
            // (with weekday) and, more recently, "9 май 2017 г." (no weekday, "година" suffix). Accept all
            // of them so a cosmetic change to the byline does not take the whole source down again.
            var formats = new[] { "d MMMM yyyy 'г.'", "d MMMM yyyy 'г'", "d MMMM yyyy", "dddd, d MMMM yyyy" };
            var time = DateTime.ParseExact(timeAsString, formats, new CultureInfo("bg-BG"), DateTimeStyles.None);

            var imageElement = document.QuerySelector(".post-content img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector(".post-content");
            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
