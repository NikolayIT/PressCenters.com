namespace PressCenters.Services.Sources.Ministries
{
    public class MeGovernmentBgNewsSource : MeGovernmentBgBaseSource
    {
        protected override string NewsListUrl => "bg/news.html";

        protected override int NewsListPagesCount => 63;
    }
}
