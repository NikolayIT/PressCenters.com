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

        // bfunion.bg 403s .NET's direct fetch (net10.0 fingerprint); the Cloudflare relay reaches it, and
        // that relay accepts .NET only over HTTP/2.
        public override bool UseProxy => true;

        public override bool UseHttp2 => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(string.Empty, ".heroItem a, .infoArticle a, .infoItem a", "/news/", 5, throwOnEmpty: false);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 52000; i >= 40000; i--)
            {
                var remoteNews = this.GetPublication($"{this.BaseUrl}news/{i}/0");
                if (remoteNews == null)
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
            var titleElement = document.QuerySelector("h3.big");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".introTop__inner-links p");
            var timeAsString = timeElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "d MMMM yyyy HH:mm", new CultureInfo("bg-BG"));

            var imageElement = document.QuerySelector("meta[property='og:image']");
            var imageUrl = imageElement?.GetAttribute("content");

            var contentElement = document.QuerySelector(".introArticle");
            if (contentElement == null)
            {
                return null;
            }

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
