namespace PressCenters.Services.Sources.Ministries
{
    public class MhGovernmentBgNewsSource : MhGovernmentBgBaseSource
    {
        protected override string NewsListUrl => "bg/novini/aktualno/";

        protected override int NewsListPagesCount => 600;
    }
}
