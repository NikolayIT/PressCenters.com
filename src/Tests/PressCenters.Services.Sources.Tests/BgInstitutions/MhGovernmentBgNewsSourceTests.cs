namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources;
    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class MhGovernmentBgNewsSourceTests
    {
        [Fact]
        public void ExtractIdFromPressUrlShouldWorkCorrectly()
        {
            var provider = new MhGovernmentBgNewsSource();
            var result = provider.ExtractIdFromUrl("http://www.mh.government.bg/bg/novini/epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-strana16-03");
            Assert.Equal("epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-strana16-03", result);
        }

        [Fact]
        public void ExtractIdFromPressUrlShouldWorkCorrectlyWithSlashAtTheEnd()
        {
            var provider = new MhGovernmentBgNewsSource();
            var result = provider.ExtractIdFromUrl("http://www.mh.government.bg/bg/novini/aktualno/komisiyata-po-izgotvyane-na-nacionalna-zdravna-kar/");
            Assert.Equal("aktualno/komisiyata-po-izgotvyane-na-nacionalna-zdravna-kar", result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.mh.government.bg/bg/novini/aktualno/stanovishe-na-pacientski-organizacii-zaedno-s-teb-/";
            var provider = new MhGovernmentBgNewsSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Становище на пациентски организации „Заедно с теб“ относно ваксината „Пентаксим“", news.Title);
            Assert.Null(news.ShortContent);
            Assert.Contains("Пациентски организации „Заедно с теб“", news.Content);
            Assert.Contains("Становище на пациентски организации „Заедно с теб“ относно ваксината „Пентаксим“", news.Content);
            Assert.Equal("http://www.mh.government.bg/static/images/Ministry_of_Health-heraldic.95741c3e92f7.svg", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 22, 9, 49, 2), news.PostDate);
            Assert.Equal("aktualno/stanovishe-na-pacientski-organizacii-zaedno-s-teb-", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteEpidemicNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.mh.government.bg/bg/novini/epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-st-2016-01/";
            var provider = new MhGovernmentBgEpidemicSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Справка за епидемичната обстановка в страната за периода 04.12.2015 г. - 10.01.2016 г.", news.Title);
            Assert.Null(news.ShortContent);
            Assert.Contains("През изминалата седмица са регистрирани общо 6", news.Content);
            Assert.Contains("През изминалата седмица не са регистрирани хранителни взривове и взривове от вътреболнични инфекции.", news.Content);
            Assert.Equal("http://www.mh.government.bg/static/images/Ministry_of_Health-heraldic.95741c3e92f7.svg", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 15, 15, 44, 0), news.PostDate);
            Assert.Equal("epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-st-2016-01", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MhGovernmentBgNewsSource();
            var result = provider.GetLatestPublications(new LocalPublicationsInfo { LastLocalId = string.Empty });

            Assert.True(result.News.Count() >= 10);
        }

        [Fact]
        public void GetEpidemicNewsShouldReturnResults()
        {
            var provider = new MhGovernmentBgEpidemicSource();
            var result = provider.GetLatestPublications(new LocalPublicationsInfo { LastLocalId = string.Empty });

            Assert.True(result.News.Count() >= 10);
        }
    }
}
