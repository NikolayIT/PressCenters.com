namespace PressCenters.Services.Sources.BgNgos
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    /// <summary>
    /// Институт за пазарна икономика.
    /// </summary>
    public class ImeBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://ime.bg/";

        // ime.bg is behind Cloudflare's bot challenge: the Azure relays are always challenged, the Cloudflare relay and a
        // direct fetch let .NET through with the revocation-checking TLS handshake, over HTTP/1.1 (see BaseSource).
        public override bool UseProxy => true;

        public override bool CheckCertificateRevocation => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("articles", "article a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            // The weekly bulletin issues, newest first (issue 1318 is from 2026-10-16).
            for (var i = 1318; i >= 1; i--)
            {
                var news = this.GetPublications($"articles/bulletin-issue/{i}/", "article a", throwOnEmpty: false);
                Console.WriteLine($"№{i} => {news.Count} news ({news.DefaultIfEmpty().Min(x => x?.PostDate)} - {news.DefaultIfEmpty().Max(x => x?.PostDate)})");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            // A WordPress block theme since 2026: the title, date and body are core post blocks, and the featured image
            // is the background of the cover block above the title (absent when the article has none; og:image is
            // then the site logo, so it is not used).
            var title = document.QuerySelector("h1.wp-block-post-title")?.TextContent;
            var timeAsString = document.QuerySelector(".wp-block-post-date time")?.GetAttribute("datetime");
            var contentElement = document.QuerySelector(".entry-content.wp-block-post-content");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(timeAsString) || contentElement == null)
            {
                return null;
            }

            // The site's clock time: its offset is a fixed +03:00 even in winter ("2019-01-04T00:00:00+03:00"), so
            // converting it to local time would move winter posts an hour back, to the previous day at midnight.
            var time = DateTimeOffset.Parse(timeAsString, CultureInfo.InvariantCulture).DateTime;

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector("img.wp-block-cover__image-background");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
