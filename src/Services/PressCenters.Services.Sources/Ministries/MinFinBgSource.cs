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

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news", ".news_list div a", "bg/news");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 605; i++)
            {
                var news = this.GetPublications($"bg/news?p={i}", ".news_list div a", "bg/news");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("#content h3");
            if (titleElement == null)
            {
                return null;
            }

            var title = new CultureInfo("bg-BG", false).TextInfo.ToTitleCase(
                titleElement.TextContent?.ToLower() ?? string.Empty);
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var timeElement = document.QuerySelector("#content .single-news-date");
            var timeAsString = timeElement?.TextContent?.Trim();
            timeAsString = timeAsString.Replace(".-0001", ".1999");
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy г.", CultureInfo.InvariantCulture);

            var contentElement = document.QuerySelector("#content .inner-content");
            contentElement.RemoveRecursively(document.QuerySelector("#social"));

            var imageElement = contentElement?.QuerySelector("#content img");
            var imageUrl = imageElement?.GetAttribute("src");

            contentElement.RemoveRecursively(timeElement);
            contentElement.RemoveRecursively(imageElement);
            contentElement.RemoveRecursively(document.QuerySelector("#content .news_images")); // All images
            contentElement.RemoveRecursively(contentElement?.QuerySelector("script")); // All scripts
            contentElement.RemoveRecursively(contentElement?.QuerySelector(".page-share")); // Share links
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;
            if (string.IsNullOrWhiteSpace(content) || content == "<p></p>")
            {
                return null;
            }

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
