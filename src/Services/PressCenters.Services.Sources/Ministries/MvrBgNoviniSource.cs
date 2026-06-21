namespace PressCenters.Services.Sources.Ministries
{
    public class MvrBgNoviniSource : MvrBgBaseSource
    {
        public override string NewsListUrl { get; } = "press/актуална-информация/актуална-информация/новини";

        public override int NewsListPagesCount { get; } = 800;
    }
}
