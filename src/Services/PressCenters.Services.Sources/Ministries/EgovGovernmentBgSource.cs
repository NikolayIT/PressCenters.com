namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на иновациите и дигиталната трансформация (the former Министерство на електронното управление).
    /// </summary>
    public class EgovGovernmentBgSource : BaseSource
    {
        // The site abbreviates the months to three letters ("21 Окт 2026", "31 Мар 2026"); .NET's bg-BG has "март".
        private static readonly CultureInfo DateCulture = CreateDateCulture();

        // МЕУ merged with МИР into МИДТ: egov.government.bg redirects to midt.gov.bg, whose news (/bg/news/{id})
        // continue МЕУ's and also hold МИР's back to 2021.
        public override string BaseUrl => "https://midt.gov.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/news", "a.card--news", "/bg/news/", 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 125; i++)
            {
                var news = this.GetPublications($"bg/news?p={i}", "a.card--news", "/bg/news/", throwOnEmpty: false);
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    // МИДТ took over on 2026-05-08 (the minister's handover); the older news here are МИР's. МЕУ's own
                    // news after 2025-03-27, the last one in the database, went away with egov.government.bg.
                    if (remoteNews.PostDate < new DateTime(2026, 5, 8))
                    {
                        yield break;
                    }

                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var title = document.QuerySelector("h1.page-title")?.TextContent;
            var timeAsString = document.QuerySelector(".page-meta span")?.TextContent?.Trim();
            var contentElement = document.QuerySelector(".page-content");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(timeAsString) || contentElement == null)
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "d MMM yyyy", DateCulture);

            // The lead image, without its ?h=650 resize query (the other img in .page-thumb is its blurred backdrop).
            var imageElement = document.QuerySelector(".page-thumb > img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src")?.Split('?')[0]);

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }

        private static CultureInfo CreateDateCulture()
        {
            var culture = (CultureInfo)CultureInfo.GetCultureInfo("bg-BG").Clone();
            var months = new[] { "яну", "фев", "мар", "апр", "май", "юни", "юли", "авг", "сеп", "окт", "ное", "дек", string.Empty };
            culture.DateTimeFormat.AbbreviatedMonthNames = months;
            culture.DateTimeFormat.AbbreviatedMonthGenitiveNames = months;
            return culture;
        }
    }
}
