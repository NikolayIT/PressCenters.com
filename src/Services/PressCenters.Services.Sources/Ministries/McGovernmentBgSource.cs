namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на културата. Migrated from the old www.mc.government.bg PHP site to a new WordPress
    /// (Elementor) site on mc.government.bg; news live at /новини/ with /новини/{slug}/ articles.
    /// </summary>
    public class McGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://mc.government.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("новини/", "h4.elementor-heading-title a", "новини/", 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var page = 1; page <= 400; page++)
            {
                var address = page == 1 ? "новини/" : $"новини/page/{page}/";
                var news = this.GetPublications(address, "h4.elementor-heading-title a", "новини/", throwOnEmpty: false);
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
            var title = document.QuerySelector("meta[property='og:title']")?.GetAttribute("content")?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            // og:title carries a " - Министерство на културата…" site suffix; drop it.
            var suffixIndex = title.IndexOf(" - Министерство", StringComparison.Ordinal);
            if (suffixIndex > 0)
            {
                title = title.Substring(0, suffixIndex).Trim();
            }

            var dateText = document.QuerySelectorAll(".elementor-icon-box-title")
                .Select(x => x.TextContent.Trim())
                .FirstOrDefault(x => Regex.IsMatch(x, @"^\d{1,2}\.\d{1,2}\.\d{4}$"));
            if (dateText == null)
            {
                return null;
            }

            var time = DateTime.ParseExact(dateText, "d.M.yyyy", CultureInfo.InvariantCulture);

            var imageUrl = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content");

            var contentElement = document.QuerySelector(".elementor-widget-theme-post-content");
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
