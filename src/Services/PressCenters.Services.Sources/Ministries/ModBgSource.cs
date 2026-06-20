namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    using PressCenters.Common;

    /// <summary>
    /// Министерство на отбраната.
    /// </summary>
    public class ModBgSource : BaseSource
    {
        public override string BaseUrl => "https://www.mod.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            // The listing has no <a> links; each card navigates via onclick="location.href='/news<id>'".
            var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}news"));
            var links = document.QuerySelectorAll("[onclick]")
                .Select(x => x.GetAttribute("onclick"))
                .Where(x => x?.Contains("location.href='/news") == true)
                .Select(x => this.NormalizeUrl(x.GetStringBetween("location.href='", "'")))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Take(5)
                .ToList();
            return links.Select(this.GetPublication).Where(x => x != null).ToList();
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var lastSegment = new Uri(url.Trim().Trim('/')).Segments[^1];
            return new string(lastSegment.Where(char.IsDigit).ToArray());
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var title = document.QuerySelector("meta[property='og:title']")?.GetAttribute("content")?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            // The publish date sits as plain text right after the title heading (e.g. "21.09.2012 г.").
            var headerText = document.QuerySelector("h3")?.ParentElement?.TextContent ?? string.Empty;
            var dateMatch = Regex.Match(headerText, @"\d{2}\.\d{2}\.\d{4}");
            var time = dateMatch.Success
                           ? DateTime.ParseExact(dateMatch.Value, "dd.MM.yyyy", CultureInfo.InvariantCulture)
                           : DateTime.Now;

            // og:image is the article photo when present, otherwise it falls back to the site logo.
            var imageUrl = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content")?.Trim();
            if (imageUrl?.Contains("/uploads/") != true)
            {
                imageUrl = null;
            }

            var contentElement = document.QuerySelector(".col-md-10.offset-md-1");
            if (contentElement == null)
            {
                return null;
            }

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
