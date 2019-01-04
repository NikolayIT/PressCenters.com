namespace PressCenters.Services.Sources.BgInstitutions
{
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public class NsiBgNewsSource : NsiBgBaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetLatestPublications("bg/events_list", ".view-content .views-field-title a");

        protected override string GetContent(IHtmlCollection<IElement> imageAndContent)
        {
            return imageAndContent[1].InnerHtml;
        }
    }
}
