namespace PressCenters.Sources.BgInstitutions
{
    public class MhGovernmentBgEpidemicSource : MhGovernmentBgBaseSource
    {
        protected override string GetNewsListUrl()
        {
            return "http://www.mh.government.bg/bg/novini/epidemichna-obstanovka/";
        }
    }
}
