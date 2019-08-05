namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    /// <summary>
    /// Министерство на земеделието, храните и горите.
    /// </summary>
    public class MzhGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.mzh.government.bg/";

        protected override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/press-center/novini/", ".news h2 a", "bg/press-center/novini");

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 731; i++)
            {
                var news = this.GetPublications($"bg/press-center/novini/?page={i}", ".news h2 a", "bg/press-center/novini");
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            return uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1];
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1");
            var title = titleElement?.TextContent;

            var timeAsString = document.QuerySelector(".newsdate li time").Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);
            if (time.Minute == 0 && (time.Hour == 2 || time.Hour == 3))
            {
                time = time.Date;
            }

            var imageElement = document.QuerySelector(".col-md-8 img.img-responsive");
            var imageUrl = imageElement?.GetAttribute("src") ?? "/images/sources/mzh.government.bg.png";

            var contentElement = document.QuerySelector(".single_news");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
