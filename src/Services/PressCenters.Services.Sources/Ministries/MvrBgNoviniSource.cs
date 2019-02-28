namespace PressCenters.Services.Sources.Ministries
{
    public class MvrBgNoviniSource : MvrBgBaseSource
    {
        public override string NewsListUrl { get; } = "press/актуална-информация/актуална-информация/новини";

        public override string NewsLinkSelector { get; } =
            ".article__list .article .article__description a.link--clear";

        public override int NewsListPagesCount { get; } = 800;
    }
}
