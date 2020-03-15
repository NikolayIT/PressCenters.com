namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Народно събрание на Република България.
    /// </summary>
    public class ParliamentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.parliament.bg/";

        protected override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news", ".frontList li.padding1 a", count: 3);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 4625; i++)
            {
                var remoteNews = this.GetPublication($"{this.BaseUrl}bg/news/ID/{i}");
                if (remoteNews == null)
                {
                    continue;
                }

                Console.WriteLine($"№{i} => {remoteNews.PostDate.ToShortDateString()} => {remoteNews.Title}");
                yield return remoteNews;
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var timeElement = document.QuerySelector(".markframe .marktitle .dateclass");
            var timeAsString = timeElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(timeAsString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var titleElement = document.QuerySelector(".markframe .marktitle");
            if (titleElement == null)
            {
                return null;
            }

            titleElement.RemoveRecursively(timeElement);
            var title = titleElement.TextContent;

            var imageElement = document.QuerySelector(".markframe .markcontent img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/parliament.bg.jpg";

            var contentElement = document.QuerySelector(".markframe .markcontent");
            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
