namespace PressCenters.Services.Sources.BgInstitutions
{
    public class MhGovernmentBgParlamentarenKontrolSource : MhGovernmentBgBaseSource
    {
        protected override string NewsListUrl => $"{this.BaseUrl}bg/novini/parlamentaren-kontrol/";

        protected override int NewsListPagesCount => 50;
    }
}
