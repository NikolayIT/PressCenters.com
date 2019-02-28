namespace PressCenters.Services.Sources.BgInstitutions
{
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public class NsiBgNewsSource : NsiBgBaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("bg/events_list", ".view-content .views-field-title a", count: 5);

        protected override string GetContent(IHtmlCollection<IElement> imageAndContent)
        {
            return imageAndContent.Length == 1 ? string.Empty : imageAndContent[1].InnerHtml;
        }
    }
}
