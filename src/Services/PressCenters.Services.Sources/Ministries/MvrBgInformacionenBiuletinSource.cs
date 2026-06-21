namespace PressCenters.Services.Sources.Ministries
{
    public class MvrBgInformacionenBiuletinSource : MvrBgBaseSource
    {
        public override string NewsListUrl { get; } = "press/актуална-информация/актуална-информация/информационен-бюлетин";

        public override int NewsListPagesCount { get; } = 150;
    }
}
