namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Български футболен съюз.
    /// </summary>
    public class BfunionBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://bfunion.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(
                $"archive/{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/0/0",
                ".newsItem .entry-title a",
                count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 40000; i <= 46260; i++)
            {
                var remoteNews = this.GetPublication($"{this.BaseUrl}news/{i}/0");
                if (remoteNews == null || remoteNews.PostDate.Date == DateTime.UtcNow.Date
                                       || remoteNews.PostDate.Date == DateTime.UtcNow.AddDays(-1).Date)
                {
                    continue;
                }

                Console.WriteLine($"№{i} => {remoteNews.PostDate.ToShortDateString()} => {remoteNews.Title}");
                yield return remoteNews;
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[uri.Segments.Length - 2].Trim('/');
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".post-content .entry-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".post-content .date");
            var timeAsString = timeElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy HH:mm", new CultureInfo("bg-BG"));

            var imageElement = document.QuerySelector(".entry-thumbnail img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/bfunion.bg.png";

            var contentElement = document.QuerySelector(".tr-details");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
