namespace PressCenters.Services.Sources.BgInstitutions
{
    public class MhGovernmentBgNewsSource : MhGovernmentBgBaseSource
    {
        protected override string NewsListUrl => $"{this.BaseUrl}bg/novini/aktualno/";

        protected override int NewsListPagesCount => 600;
    }
}
