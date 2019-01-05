namespace PressCenters.Services.Sources.Ministries
{
    public class MhGovernmentBgEpidemicSource : MhGovernmentBgBaseSource
    {
        protected override string NewsListUrl => "bg/novini/epidemichna-obstanovka/";

        protected override int NewsListPagesCount => 60;
    }
}
