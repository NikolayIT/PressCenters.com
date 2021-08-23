namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;
    using AngleSharp.Xml.Parser;

    public abstract class NsiBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://nsi.bg/";

        public IEnumerable<RemoteNews> GetLatestPublicationsFromXml(string xmlUrl)
        {
            var parser = new XmlParser();
            var document = parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}{xmlUrl}"));
            var links = document.QuerySelectorAll("item link").Select(x => this.NormalizeUrl(x.TextContent)).Take(5);
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));

            // Find first segment that looks like a numeric id
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
            var title = document.QuerySelector("h1.page-title").TextContent.Trim();
            var imageAndContent = document.QuerySelector("article.node");

            var imageElement = imageAndContent.QuerySelector("img");
            var imageUrl = imageElement?.Attributes["src"]?.Value;
            imageAndContent.RemoveRecursively(imageElement);

            var timeElement = imageAndContent.QuerySelector(".node__meta span");
            var timeString = timeElement.TextContent.Replace("Публикувано на: ", string.Empty).Trim();
            var time = DateTime.ParseExact(timeString, "dd.MM.yyyy - HH:mm", CultureInfo.InvariantCulture);
            imageAndContent.RemoveRecursively(timeElement);

            imageAndContent.RemoveRecursively(imageAndContent.QuerySelector("span.addtoany_list"));
            var content = imageAndContent?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
