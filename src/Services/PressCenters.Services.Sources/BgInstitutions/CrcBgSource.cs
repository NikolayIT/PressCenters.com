namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Комисия за регулиране на съобщенията.
    /// </summary>
    public class CrcBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://crc.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/novini", ".blog-post .title a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 43; page <= 139; page++)
            {
                var news = this.GetPublications($"bg/novini?page={page}", ".blog-post .title a");
                Console.WriteLine($"Page {page} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[uri.Segments.Length - 2].Trim('/');
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".section-heading-primary");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector("time");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd/MM/yy, HH:mm", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".featured-image img");
            var imageUrl = imageElement?.GetAttribute("src");
            if (imageUrl.EndsWith("/images/news/news-1.jpg"))
            {
                imageUrl = null;
            }

            var contentElement = document.QuerySelector(".item-text-content");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
