namespace PressCenters.Services.Sources.Ministries
{
    public class MoewGovernmentBgRegionalNewsSource : MoewGovernmentBgBaseSource
    {
        protected override string NewsListUrl => "bg/prescentur/regionalni-novini/";

        protected override int NewsListPagesCount => 91;
    }
}
