namespace PressCenters.Services.Sources.BgInstitutions
{
    public class MhGovernmentBgEpidemicSource : MhGovernmentBgBaseSource
    {
        protected override string GetNewsListUrl()
        {
            return $"{this.BaseUrl}bg/novini/epidemichna-obstanovka/";
        }
    }
}
