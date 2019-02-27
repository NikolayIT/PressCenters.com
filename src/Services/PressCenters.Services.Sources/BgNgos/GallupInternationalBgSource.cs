namespace PressCenters.Services.Sources.BgNgos
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using AngleSharp.Dom;

    /// <summary>
    /// Институт за пазарна икономика.
    /// </summary>
    public class GallupInternationalBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.gallup-international.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/Публикации/", "a.contentpagetitle, a.blogsection", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 110; i <= 500; i++)
            {
                Console.Title = i.ToString();
                var remoteNews = this.GetPublication($"{this.BaseUrl}bg/Публикации/1-2000/{i}-news");
                if (remoteNews == null)
                {
                    continue;
                }

                Console.WriteLine($"№{remoteNews.RemoteId} => {remoteNews.PostDate.ToShortDateString()} => {remoteNews.Title}");
                yield return remoteNews;
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var slashIndex = url.LastIndexOf("/", StringComparison.Ordinal);
            var dashIndex = url.IndexOf("-", slashIndex, StringComparison.Ordinal);
            return url.Substring(slashIndex + 1, dashIndex - slashIndex - 1);
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            if (document.QuerySelector(".pagenav") == null)
            {
                // Not a news but a page
                return null;
            }

            var titleElement = document.QuerySelector(".contentpagetitle");
            var title = titleElement?.TextContent;
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var timeElement = document.QuerySelector(".createdate");
            var timeAsString = timeElement?.TextContent?.Trim()?.ToLower();
            var time = DateTime.ParseExact(timeAsString, "dddd, dd MMMM yyyy HH:mm", CultureInfo.GetCultureInfo("bg-BG"));

            var contentElement = document.QuerySelector(".contentpaneopen td[valign=top]:not([class=createdate])");
            if (contentElement == null)
            {
                throw new Exception("Content element is null");
            }

            contentElement.RemoveRecursively(contentElement.QuerySelector(".pagenav"));
            contentElement.RemoveRecursively(contentElement.QuerySelector(".pagenav"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml;

            return new RemoteNews(title, content, time, "/images/sources/gallup-international.bg.png");
        }
    }
}
