namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Комисия за противодействие на корупцията и за отнемане на незаконно придобитото имущество.
    /// </summary>
    public class CiafGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.ciaf.government.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("news/", "ul.listNews li h2 a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 45; i++)
            {
                var news = this.GetPublications($"news/?page={i}", "ul.listNews li h2 a");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => url.Split("-")[^1].TrimEnd('/');

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".detailNews h1");
            if (titleElement == null)
            {
                return null;
            }

            var title = new CultureInfo("bg-BG", false).TextInfo.ToTitleCase(
                titleElement.TextContent?.Trim()?.ToLower() ?? string.Empty);

            var timeElement = document.QuerySelector(".detailNews .dateNews");
            var timeAsString = timeElement.TextContent;
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy | HH:mm", CultureInfo.InvariantCulture);

            var contentElement = document.QuerySelector(".detailNews .txtNews");
            contentElement.RemoveRecursively(document.QuerySelector(".detailNews .iconNews"));
            contentElement.RemoveRecursively(document.QuerySelector(".detailNews a.view_more"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".detailNews .imgNews img");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
