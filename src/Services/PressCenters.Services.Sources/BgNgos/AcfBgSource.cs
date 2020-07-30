namespace PressCenters.Services.Sources.BgNgos
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    /// <summary>
    /// Антикорупционен фонд.
    /// </summary>
    public class AcfBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://acf.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/pres-syobshteniya/", ".padding-bottom-80 a.white-red-btn");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 26; i++)
            {
                var news = this.GetPublications($"bg/pres-syobshteniya/page/{i}/", ".padding-bottom-80 a.white-red-btn");
                Console.WriteLine($"№{i} => {news.Count} news ({news.DefaultIfEmpty().Min(x => x?.PostDate)} - {news.DefaultIfEmpty().Max(x => x?.PostDate)})");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1");
            var title = titleElement?.TextContent;
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var timeElement = document.QuerySelector("time");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var contentElement = document.QuerySelector("div.line-height-26");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            var imageElement = document?.QuerySelector("figure.padding-bottom-30 img");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
