namespace PressCenters.Sources.BgInstitutions
{
    public class MhGovernmentBgNewsSource : MhGovernmentBgBaseSource
    {
        protected override string GetNewsListUrl()
        {
            return "http://www.mh.government.bg/bg/novini/aktualno/";
        }
    }
}
