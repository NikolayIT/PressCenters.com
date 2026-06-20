namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;
    using AngleSharp.Xml.Parser;

    public abstract class NsiBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://nsi.bg/";

        public override bool UseProxy => true;

        public IEnumerable<RemoteNews> GetLatestPublicationsFromXml(string xmlUrl)
        {
            var parser = new XmlParser();
            var content = this.ReadStringFromUrl($"{this.BaseUrl}{xmlUrl}");
            var document = parser.ParseDocument(content);
            var links = document.QuerySelectorAll("item link").Select(x => this.NormalizeUrl(x.TextContent)).Take(5);
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            var lastSegment = uri.Segments[^1].Trim('/');

            // Current URLs end with "...-<id>" (e.g. /news/some-slug-9642, /press-release/.../-9338).
            var trailingId = Regex.Match(lastSegment, @"-(\d+)$");
            if (trailingId.Success)
            {
                return trailingId.Groups[1].Value;
            }

            // Older URLs carried the id as its own path segment (e.g. /bg/content/13854/...).
            for (var i = uri.Segments.Length - 1; i >= 0; i--)
            {
                var segment = uri.Segments[i]?.Trim('/') ?? string.Empty;
                if (Regex.IsMatch(segment, @"^\d+$"))
                {
                    return segment;
                }
            }

            return base.ExtractIdFromUrl(url);
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".page-title-content h2");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".entry-meta");
            var timeString = timeElement?.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(timeString))
            {
                return null;
            }

            var time = DateTime.ParseExact(timeString, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".newsImgHolder img");
            var imageUrl = this.NormalizeUrl(imageElement?.GetAttribute("src"));

            // The body lives in one or more ".article-content" blocks (the lead one also carries the date,
            // the duplicated title and any PDF download links; the image gallery and the prev/next nav are
            // separate siblings we deliberately skip). Keep the PDF links and the text, drop date and <h3>.
            var contentBuilder = new StringBuilder();
            foreach (var part in document.QuerySelectorAll(".article-content"))
            {
                foreach (var meta in part.QuerySelectorAll(".entry-meta, h3"))
                {
                    part.RemoveRecursively(meta);
                }

                this.NormalizeUrlsRecursively(part);
                contentBuilder.Append(part.InnerHtml);
            }

            var content = contentBuilder.ToString().Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
