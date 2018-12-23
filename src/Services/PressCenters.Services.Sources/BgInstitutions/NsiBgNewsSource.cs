namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;

    using AngleSharp.Dom;

    public class NsiBgNewsSource : NsiBgBaseSource
    {
        protected override string GetNewsListUrl()
        {
            return "http://www.nsi.bg/bg/events_list";
        }

        protected override string GetContent(IHtmlCollection<IElement> imageAndContent)
        {
            return imageAndContent[1].InnerHtml;
        }

        protected override string GetImageUrl(IHtmlCollection<IElement> imageAndContent)
        {
            var imageUrl = imageAndContent[0].QuerySelector("img")?.Attributes["src"]?.Value;
            return imageUrl;
        }

        protected override string ChooseUrlSegmentForId(Uri uri)
        {
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                         ? uri.Segments[uri.Segments.Length - 2]
                         : uri.Segments[uri.Segments.Length - 3];
            return id;
        }
    }
}
