namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на финансите.
    /// </summary>
    public class MinFinBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.minfin.bg/";

        // minfin.bg 403s .NET's direct fetch (net10.0 fingerprint); the Cloudflare relay reaches it over HTTP/2.
        public override bool UseProxy => true;

        public override bool UseHttp2 => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news", ".news_list div a", "bg/news", 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 605; i++)
            {
                var news = this.GetPublications($"bg/news?p={i}", ".news_list div a", "bg/news", throwOnEmpty: false);
                Console.WriteLine($"Page {i} => {news.Count} news");
                if (news.Count == 0)
                {
                    break;
                }

                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("#content h1");
            var title = titleElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var timeElement = document.QuerySelector("#content .single-news-date");
            var timeAsString = timeElement?.TextContent?.Trim();
            timeAsString = timeAsString?.Replace(".-0001", ".1999");
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy г.", CultureInfo.InvariantCulture);

            var contentElement = document.QuerySelector("#content .inner-content");
            if (contentElement == null)
            {
                return null;
            }

            var imageElement = document.QuerySelector("meta[property='og:image']");
            var imageUrl = imageElement?.GetAttribute("content");

            contentElement.RemoveRecursively(document.QuerySelector("#social"));
            contentElement.RemoveRecursively(timeElement);
            contentElement.RemoveRecursively(document.QuerySelector("#content .news_images"));
            contentElement.RemoveRecursively(contentElement.QuerySelector("script"));
            contentElement.RemoveRecursively(contentElement.QuerySelector(".page-share"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml;
            if (string.IsNullOrWhiteSpace(content) || content == "<p></p>")
            {
                return null;
            }

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
