namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    /// <summary>
    /// Омбудсман на Република България.
    /// </summary>
    public class OmbudsmanBgSource : BaseSource
    {
        // The new CMS took the old site's news over under new ids; those, up to this date, are in the database under
        // the old site's ids.
        private static readonly DateTime MigratedNewsEnd = new DateTime(2026, 9, 11);

        // A new CMS since 2026-09: the news are ?go=news&p=detail&newsId={id}, and the old /bg/n/{slug}-{id} URLs
        // redirect to the home page.
        public override string BaseUrl { get; } = "https://www.ombudsman.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("?go=news", "article a", "newsId=", 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 300; i++)
            {
                var news = this.GetPublications($"?go=news&page={i}", "article a", "newsId=", throwOnEmpty: false);
                Console.WriteLine($"№{i} => {news.Count} news ({news.DefaultIfEmpty().Min(x => x?.PostDate)} - {news.DefaultIfEmpty().Max(x => x?.PostDate)})");
                if (news.Count == 0)
                {
                    break;
                }

                foreach (var remoteNews in news)
                {
                    if (remoteNews.PostDate < MigratedNewsEnd)
                    {
                        yield break;
                    }

                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => this.GetUrlParameterValue(url, "newsId");

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var title = document.QuerySelector(".news_grid_main article h1")?.TextContent;
            var timeAsString = document.QuerySelector(".news_grid_main article time")?.GetAttribute("datetime");
            var contentElement = document.QuerySelector(".news_grid_main article .post_txt");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(timeAsString) || contentElement == null)
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var imageUrl = document.QuerySelector(".news_grid_main article figure img")?.GetAttribute("src");

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
