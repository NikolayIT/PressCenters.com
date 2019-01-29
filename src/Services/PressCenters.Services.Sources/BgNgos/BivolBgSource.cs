namespace PressCenters.Services.Sources.BgNgos
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;

    using AngleSharp.Dom;
    using AngleSharp.Parser.Xml;

    using PressCenters.Common;

    /// <summary>
    /// „Биволъ“.
    /// </summary>
    public class BivolBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://bivol.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var parser = new XmlParser();
            var document = parser.Parse(
                this.ReadStringFromUrl($"{this.BaseUrl}feed").Replace(
                    "xmlns:georss=\"http://www.georss.org/georss\"",
                    string.Empty));
            var links = document.QuerySelectorAll("item link").Select(x => this.NormalizeUrl(x.TextContent)).Take(5);
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 250; i++)
            {
                var news = this.GetPublications($"page/{i}", "h2.entry-title a");
                Console.WriteLine($"№{i} => {news.Count} news ({news.DefaultIfEmpty().Min(x => x?.PostDate)} - {news.DefaultIfEmpty().Max(x => x?.PostDate)})");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) =>
            WebUtility.UrlDecode(url.GetLastStringBetween("/", ".html"));

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelectorAll("h1.post-title").LastOrDefault();
            var title = titleElement?.TextContent;
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var timeAsString = document.QuerySelector(".post-box-meta-single time.entry-date")?.Attributes["datetime"]?.Value;
            var time = DateTime.Parse(timeAsString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

            var imageElement = document.QuerySelector(".post-image a");
            var imageUrl = imageElement?.GetAttribute("href") ?? "/images/sources/bivol.bg.jpg";

            var contentElement = document.QuerySelector("#modal-ready");
            contentElement.RemoveRecursively(document.QuerySelector("#modal-ready .dkpdf-button-container"));
            contentElement.RemoveRecursively(document.QuerySelector("#modal-ready div:has(figure.wp-block-pullquote)"));
            contentElement.RemoveRecursively(document.QuerySelector("#modal-ready div:has(figure.wp-block-pullquote)"));
            contentElement.RemoveRecursively(document.QuerySelector("#modal-ready figure.wp-block-pullquote"));
            contentElement.RemoveRecursively(document.QuerySelector("#modal-ready figure.wp-block-pullquote"));
            contentElement.RemoveRecursively(document.QuerySelector("#modal-ready div:has(script)"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
