namespace PressCenters.Services.Sources.Ministries
{
    public class MeGovernmentBgHotNewsSource : MeGovernmentBgBaseSource
    {
        protected override string NewsListUrl => "bg/hot-news.html";

        protected override int NewsListPagesCount => 18;
    }
}
