namespace PressCenters.Services.Sources.Ministries
{
    public class MhGovernmentBgMinisterskiSuvetSource : MhGovernmentBgBaseSource
    {
        protected override string NewsListUrl => "bg/novini/ministerski-savet/";

        protected override int NewsListPagesCount => 50;
    }
}
