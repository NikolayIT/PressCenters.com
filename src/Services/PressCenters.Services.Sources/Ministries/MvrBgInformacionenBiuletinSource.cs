namespace PressCenters.Services.Sources.Ministries
{
    public class MvrBgInformacionenBiuletinSource : MvrBgBaseSource
    {
        public override string NewsListUrl { get; } = "press/актуална-информация/актуална-информация/информационен-бюлетин";

        public override string NewsLinkSelector { get; } = ".article__list a.link--clear";

        public override int NewsListPagesCount { get; } = 150;
    }
}
