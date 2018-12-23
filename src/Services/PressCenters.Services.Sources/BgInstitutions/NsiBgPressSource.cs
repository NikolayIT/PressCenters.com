namespace PressCenters.Sources.BgInstitutions
{
    using System;

    using AngleSharp.Dom;

    public class NsiBgPressSource : NsiBgBaseSource
    {
        protected override string GetNewsListUrl()
        {
            return "http://www.nsi.bg/bg/pressreleases_list";
        }

        protected override string GetContent(IHtmlCollection<IElement> imageAndContent)
        {
            return imageAndContent[0].InnerHtml;
        }

        protected override string GetImageUrl(IHtmlCollection<IElement> imageAndContent)
        {
            return "http://www.nsi.bg/sites/default/files/styles/medium/public/files/events/images/___NSILogo_117.jpg";
        }

        protected override string ChooseUrlSegmentForId(Uri uri)
        {
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                         ? uri.Segments[uri.Segments.Length - 3]
                         : uri.Segments[uri.Segments.Length - 4];
            return id;
        }
    }
}
