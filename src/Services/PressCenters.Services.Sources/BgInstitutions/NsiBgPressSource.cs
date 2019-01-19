namespace PressCenters.Services.Sources.BgInstitutions
{
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public class NsiBgPressSource : NsiBgBaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/pressreleases_list", ".view-content .views-field-title a", count: 5);

        protected override string GetContent(IHtmlCollection<IElement> imageAndContent)
        {
            return imageAndContent[0].InnerHtml;
        }
    }
}
