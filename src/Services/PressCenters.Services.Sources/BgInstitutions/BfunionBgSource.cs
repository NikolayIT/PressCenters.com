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

        // bfunion.bg is behind Cloudflare's bot challenge: the Azure relays are always challenged, the Cloudflare relay and
        // a direct fetch let .NET through with the revocation-checking TLS handshake, over HTTP/1.1 (see BaseSource).
        public override bool UseProxy => true;

        public override bool CheckCertificateRevocation => true;

        // Each card now also carries a Facebook "share" link whose href embeds the article URL
        // (…/sharer.php?u=https://bfunion.bg/news/{id}/0), so it survives the "/news/" filter and steals a
        // slot; exclude those share anchors so only real article links are taken.
        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(
                string.Empty,
                ".heroItem a:not([href*='facebook']), .infoArticle a:not([href*='facebook']), .infoItem a:not([href*='facebook'])",
                "/news/",
                5,
                throwOnEmpty: false);

        // The news list, 10 per page, newest first. Not the ids one by one: a missing id (above the newest one, or a
        // deleted news) answers with the newest news, which got imported again under each of those ids.
        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 600; i++)
            {
                var news = this.GetPublications(
                    $"Новини?page={i}", "a[href^='https://bfunion.bg/news/']", "/news/", throwOnEmpty: false);
                Console.WriteLine($"Page {i} => {news.Count} news");
                if (news.Count == 0)
                {
                    yield break;
                }

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
