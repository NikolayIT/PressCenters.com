namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    /// <summary>
    /// Комисия за отнемане на незаконно придобитото имущество (КОНПИ). The former КПКОНПИ was split into two
    /// bodies: the asset-forfeiture commission moved to ciaf.bg (this source), the anti-corruption КПК to
    /// cacbg.bg. ciaf.bg keeps the same CMS/template as the old caciaf.bg site, just on the new domain under a
    /// /bg language path, so the article markup (article.inner-block) is unchanged.
    /// </summary>
    // TODO: Add a sibling source for the anti-corruption commission (КПК) at cacbg.bg -- the other half of the
    // former КПКОНПИ -- once that site is back online. It is currently unreachable (the host does not respond;
    // the TLS connection times out). When it is up, mirror this source: a new BaseSource subclass plus a row
    // in SourcesSeeder.
    public class CiafGovernmentBgSource : BaseSource
    {
        public override string BaseUrl => "https://ciaf.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/aktualno/novini", "article.article a", "aktualno/novini", 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 100; page++)
            {
                var news = this.GetPublications($"bg/aktualno/novini?page={page}", "article.article a", "aktualno/novini", throwOnEmpty: false);
                Console.WriteLine($"Page {page} => {news.Count} news");
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

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("article.inner-block h2");
            if (titleElement == null)
            {
                return null;
            }

            // The site now serves natural sentence-case headlines (the old caciaf.bg site was all-caps, hence
            // the previous ToTitleCase), so take the heading text as-is.
            var title = titleElement.TextContent?.Trim();

            var timeElement = document.QuerySelector("article.inner-block time.inner-block__date");
            var timeAsString = timeElement?.Attributes["datetime"]?.Value;
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.Parse(timeAsString);

            var contentElement = document.QuerySelector("article.inner-block div.text");
            if (contentElement == null)
            {
                return null;
            }

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            // Articles without an own image fall back to a site placeholder image (kept, matching prior behaviour).
            var imageElement = document.QuerySelector("article.inner-block img");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
