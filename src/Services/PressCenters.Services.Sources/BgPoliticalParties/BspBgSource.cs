namespace PressCenters.Services.Sources.BgPoliticalParties
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    using PressCenters.Common;

    public class BspBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://bsp.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("news.html", ".post-content h3 a");

        public override string ExtractIdFromUrl(string url) => url?.GetStringBetween("news/view/", "-");

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector(".post-content h2");
            var title = titleElement.TextContent.Trim();

            // Time in format: "Янр 31, 2010"
            var monthNames = new List<string> { "Янр", "Фев", "Мар", "Апр", "Май", "Юни", "Юли", "Авг", "Сеп", "Окт", "Nov", "Дек", };
            var dateAsString = document.QuerySelector(".meta_date").InnerHtml.Trim();
            var monthName = dateAsString.Substring(0, 3);
            var monthIndex = monthNames.FindIndex(x => x.ToLower() == monthName.ToLower()) + 1;
            if (monthIndex == 0)
            {
                monthIndex = DateTime.UtcNow.Month;
            }

            var dayOfMonth = dateAsString.Substring(4, 2).ToInteger();
            var year = dateAsString.Substring(dateAsString.Length - 4, 4).ToInteger();
            var time = new DateTime(year, monthIndex, dayOfMonth);

            var contentElement = document.QuerySelector(".post-content");
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            this.RemoveRecursively(contentElement, document.QuerySelector(".post-content h2"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".post-content .social-buttons"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".post-content .item_meta"));
            var content = contentElement.InnerHtml.Trim();

            var imageElement = document.QuerySelector(".post-poster img");
            var imageUrl = imageElement?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
