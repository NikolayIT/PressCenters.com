namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    /// <summary>
    /// Омбудсман на Република България.
    /// </summary>
    public class OmbudsmanBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.ombudsman.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications(string.Empty, "#leftColumn ul li a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            var lastKnownDate = new DateTime(2016, 1, 1);
            for (var i = 1; i <= 5000; i++)
            {
                // Console.Title = i.ToString();
                var remoteNews = this.GetPublication($"{this.BaseUrl}news/{i}");
                if (remoteNews == null)
                {
                    continue;
                }

                if (remoteNews.PostDate.Date == DateTime.Now.Date)
                {
                    remoteNews.PostDate = lastKnownDate;
                }

                lastKnownDate = remoteNews.PostDate;
                Console.WriteLine($"№{i} => {remoteNews.PostDate.ToShortDateString()} => {remoteNews.Title}");
                yield return remoteNews;
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            // Title
            var titleElement = document.QuerySelector(".news-content h3");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            // Time
            var timeElementAsString = string.Empty;

            var strongElement = document.QuerySelector(".news-body strong");
            if (strongElement?.TextContent?.Contains(" г.") == true && strongElement?.TextContent?.Length < 40)
            {
                timeElementAsString = strongElement.TextContent;
            }
            else
            {
                var newsAsText = document.QuerySelector(".news-body").TextContent.Trim();
                var endOfString = newsAsText.IndexOf("г.", StringComparison.Ordinal);
                if (endOfString > 0)
                {
                    timeElementAsString = newsAsText.Substring(0, endOfString + 2).Trim();
                }
            }

            var timeAsString = timeElementAsString.Trim();
            timeAsString = timeAsString.Replace("гр. София,", string.Empty);
            timeAsString = Regex.Replace(timeAsString, @"[А-Яа-я]+\,\s*", string.Empty);
            timeAsString = Regex.Replace(timeAsString, @"[А-Яа-я\s]+\(", string.Empty);
            timeAsString = timeAsString.Replace("На ", string.Empty);
            timeAsString = timeAsString.Replace("На ", string.Empty); // Not the same as the previous line
            timeAsString = timeAsString.Replace("2099", "2009");
            timeAsString = timeAsString.Replace("a", "а"); // Latin to cyrillic
            timeAsString = timeAsString.Replace("e", "е"); // Latin to cyrillic
            if (!DateTime.TryParseExact(
                timeAsString.Trim(),
                new[] { "d MMMM yyyy г.", "d MMMM yyyyг.", "dd MMMM yyyyг.", "dd.MM.yyyy г." },
                new CultureInfo("bg-BG"),
                DateTimeStyles.None,
                out var time))
            {
                //// Console.WriteLine($"!!! {this.ExtractIdFromUrl(url)} -----> {timeElementAsString}");
                time = DateTime.Now;
            }

            // Image
            var imageUrl = document.QuerySelector(".news-content > .inline-image img")?.Attributes?["src"]?.Value;

            // Content
            var contentElement = document.QuerySelector(".news-content .news-body");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
