namespace PressCenters.Services.Sources.BgInstitutions
{
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public class NsiBgPressSource : NsiBgBaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications() => this.GetLatestPublicationsFromXml("bg/pressreleases.xml");
    }
}
