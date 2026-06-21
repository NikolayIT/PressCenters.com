namespace PressCenters.Services.Sources.Ministries
{
    public class MvrBgPutnaObstanovkaSource : MvrBgBaseSource
    {
        public override string NewsListUrl { get; } = "press/актуална-информация/актуална-информация/пътна-обстановка";

        public override int NewsListPagesCount { get; } = 150;
    }
}
