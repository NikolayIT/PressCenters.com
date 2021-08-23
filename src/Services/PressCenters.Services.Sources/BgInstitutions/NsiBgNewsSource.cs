namespace PressCenters.Services.Sources.BgInstitutions
{
    using System.Collections.Generic;

    using AngleSharp.Dom;

    public class NsiBgNewsSource : NsiBgBaseSource
    {
        public override IEnumerable<RemoteNews> GetLatestPublications() => this.GetLatestPublicationsFromXml("bg/news.xml");
    }
}
