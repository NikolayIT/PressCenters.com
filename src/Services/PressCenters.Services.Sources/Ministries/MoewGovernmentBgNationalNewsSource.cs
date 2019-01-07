namespace PressCenters.Services.Sources.Ministries
{
    public class MoewGovernmentBgNationalNewsSource : MoewGovernmentBgBaseSource
    {
        protected override string NewsListUrl => "bg/prescentur/nacionalni-novini/";

        protected override int NewsListPagesCount => 263;
    }
}
