namespace PressCenters.Services.Sources.Ministries
{
    public class MvrBgAktualnoSource : MvrBgBaseSource
    {
        public override string NewsListUrl { get; } = "press/актуална-информация/актуална-информация/актуално";

        public override int NewsListPagesCount { get; } = 100;
    }
}
