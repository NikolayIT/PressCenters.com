namespace PressCenters.Services.Sources.BgInstitutions
{
    public class MhGovernmentBgNewsSource : MhGovernmentBgBaseSource
    {
        protected override string GetNewsListUrl()
        {
            return $"{this.BaseUrl}bg/novini/aktualno/";
        }
    }
}
