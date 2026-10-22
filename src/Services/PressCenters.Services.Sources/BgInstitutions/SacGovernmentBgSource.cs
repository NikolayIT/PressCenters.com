namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    public class SacGovernmentBgSource : BaseSource
    {
        // The WordPress site re-published the articles of the old Lotus Domino one under new URLs; those, up to this
        // date, are already in the database under their Domino ids.
        private static readonly DateTime MigratedArticlesEnd = new DateTime(2026, 8, 4);

        // Moved from the (now dead) www.sac.government.bg to sac.justice.bg, which replaced its Lotus Domino site with
        // WordPress in 2026-08 (the old /news/bg/{id} URLs answer 410 Gone). News: /novini/{year}/{slug}/.
        public override string BaseUrl => "https://sac.justice.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("novini/", "article a", "/novini/20", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 30; i++)
            {
                var news = this.GetPublications($"novini/page/{i}/", "article a", "/novini/20", throwOnEmpty: false);
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    if (remoteNews.PostDate < MigratedArticlesEnd)
                    {
                        yield break;
                    }

                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var title = document.QuerySelector(".page-head h1")?.TextContent;
            var timeAsString = document.QuerySelector("meta[property='article:published_time']")?.GetAttribute("content");
            var contentElement = document.QuerySelector(".article-body .content");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(timeAsString) || contentElement == null)
            {
                return null;
            }

            // A date only: the time is always midnight UTC.
            var time = DateTimeOffset.Parse(timeAsString, CultureInfo.InvariantCulture).Date;

            var imageUrl = document.QuerySelector(".article-featured img")?.GetAttribute("src");

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
