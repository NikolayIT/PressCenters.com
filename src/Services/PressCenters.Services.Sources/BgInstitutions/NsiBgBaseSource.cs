namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    public abstract class NsiBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.nsi.bg/";

        internal override string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));

            // Find first segment that looks like an numeric id
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
            var title = document.QuerySelector("h1.page__title").TextContent.Trim();
            var imageAndContent = document.QuerySelectorAll("article .field-items .field-item");

            var imageElement = document.QuerySelector(".field-name-field-event-image img");
            var imageUrl = imageElement?.Attributes["src"]?.Value
                           ?? $"{this.BaseUrl}sites/default/files/styles/medium/public/files/events/images/___NSILogo_117.jpg";
            var content = this.GetContent(imageAndContent);

            var timeElement = document.QuerySelector(".submitted span");
            var time = DateTime.Parse(timeElement.Attributes["content"].Value);

            return new RemoteNews(title, content, time, imageUrl);
        }

        protected abstract string GetContent(IHtmlCollection<IElement> imageAndContent);
    }
}
