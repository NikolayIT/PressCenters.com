namespace PressCenters.Services.Sources.Ministries
{
    public class MhGovernmentBgParlamentarenKontrolSource : MhGovernmentBgBaseSource
    {
        protected override string NewsListUrl => "bg/novini/parlamentaren-kontrol/";

        protected override int NewsListPagesCount => 50;
    }
}
