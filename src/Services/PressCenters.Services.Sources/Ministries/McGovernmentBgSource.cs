namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на културата.
    /// </summary>
    public class McGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.mc.government.bg/";

        public override Encoding Encoding
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

        public override string ExtractIdFromUrl(string url)
        {
            // TODO: Extract as base method (same as MlspBgSource)
            var matches = Regex.Matches(url, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            var parameters = matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value));
            return parameters["n"];
        }

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
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
