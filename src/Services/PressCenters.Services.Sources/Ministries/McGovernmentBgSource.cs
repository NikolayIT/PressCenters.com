namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на културата.
    /// </summary>
    public class McGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.mc.government.bg/";

        protected override Encoding Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding("windows-1251");
            }
        }

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("index.php", ".conNews a.moreLink");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 1030; i++)
            {
                var news = this.GetPublications($"index.php?p={i}", ".conNews a.moreLink");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url) => this.GetUrlParameterValue(url, "n");

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector("td.conNewsTitle");
            if (titleElement == null)
            {
                return null;
            }

            var title = new CultureInfo("bg-BG", false).TextInfo.ToTitleCase(
                titleElement.TextContent?.ToLower() ?? string.Empty);

            var timeElement = document.QuerySelector(".conNews .spanDate");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "(dd.MM.yyyy)", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".conNews").QuerySelector("img");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/mc.government.bg.jpg";

            var contentElement = document.QuerySelector(".conNews");
            contentElement.RemoveRecursively(timeElement);
            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
