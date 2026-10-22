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
        // WordPress's Bulgarian month abbreviations: "8 ян. 2026", "26 авг. 2026", "22 юли 2020".
        private static readonly CultureInfo DateCulture = CreateDateCulture();

        // The redesigned site (2026-09) links its articles by their Cyrillic slugs, while the ids stored before are the
        // same slugs transliterated ("седем-граждански-организации-предло" -> "sedem-grazhdanski-organizatsii-predlo").
        private static readonly Dictionary<char, string> Transliteration = new Dictionary<char, string>
        {
            ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e", ['ж'] = "zh", ['з'] = "z",
            ['и'] = "i", ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m", ['н'] = "n", ['о'] = "o", ['п'] = "p",
            ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u", ['ф'] = "f", ['х'] = "h", ['ц'] = "ts", ['ч'] = "ch",
            ['ш'] = "sh", ['щ'] = "sht", ['ъ'] = "a", ['ь'] = "y", ['ю'] = "yu", ['я'] = "ya",
        };

        public override string BaseUrl { get; } = "https://acf.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("current/?post_type_filter=pr", "#posts-wrapper > a");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 100; i++)
            {
                var news = this.GetPublications($"current/page/{i}/?post_type_filter=pr", "#posts-wrapper > a", throwOnEmpty: false);
                Console.WriteLine($"№{i} => {news.Count} news ({news.DefaultIfEmpty().Min(x => x?.PostDate)} - {news.DefaultIfEmpty().Max(x => x?.PostDate)})");
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

        internal override string ExtractIdFromUrl(string url) =>
            string.Concat(base.ExtractIdFromUrl(url).Select(x => Transliteration.TryGetValue(x, out var latin) ? latin : x.ToString()));

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var title = document.QuerySelector("h1.acf-post-title")?.TextContent;
            var timeAsString = document.QuerySelector(".acf-post-meta-wrapper time.acf-post-date")?.TextContent?.Trim();
            var contentElement = document.QuerySelector(".acf-post-text");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(timeAsString) || contentElement == null)
            {
                return null;
            }

            var time = DateTime.ParseExact(timeAsString, "d MMM yyyy", DateCulture);

            contentElement.RemoveRecursively(contentElement.QuerySelector(".acf-subscribe-box"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var imageUrl = document.QuerySelector("img.acf-post-featured-image")?.GetAttribute("src");

            return new RemoteNews(title, content, time, imageUrl);
        }

        private static CultureInfo CreateDateCulture()
        {
            var culture = (CultureInfo)CultureInfo.GetCultureInfo("bg-BG").Clone();
            var months = new[] { "ян.", "фев.", "мар.", "апр.", "май", "юни", "юли", "авг.", "сеп.", "окт.", "ное.", "дек.", string.Empty };
            culture.DateTimeFormat.AbbreviatedMonthNames = months;
            culture.DateTimeFormat.AbbreviatedMonthGenitiveNames = months;
            return culture;
        }
    }
}
