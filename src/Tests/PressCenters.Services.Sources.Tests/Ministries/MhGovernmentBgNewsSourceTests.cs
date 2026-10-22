namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MhGovernmentBgNewsSourceTests
    {
        [Theory]
        [InlineData("https://www.mh.government.bg/bg/novini/epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-strana16-03", "epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-strana16-03")]
        [InlineData("https://www.mh.government.bg/bg/novini/aktualno/komisiyata-po-izgotvyane-na-nacionalna-zdravna-kar/", "aktualno/komisiyata-po-izgotvyane-na-nacionalna-zdravna-kar")]
        [InlineData("https://www.mh.government.bg/bg/novini/parlamentaren-kontrol/otgovor-na-ministra-na-zdraveopazvaneto-d-r-boz-31/", "parlamentaren-kontrol/otgovor-na-ministra-na-zdraveopazvaneto-d-r-boz-31")]
        [InlineData("https://www.mh.government.bg/bg/novini/ministerski-savet/odobreni-sa-promeni-po-byudzhetite-na-sedem-minist", "ministerski-savet/odobreni-sa-promeni-po-byudzhetite-na-sedem-minist")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MhGovernmentBgNewsSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mh.government.bg/bg/novini/aktualno/4813";
            var provider = new MhGovernmentBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министърът на здравеопазването Катя Ивкова и омбудсманът Велислава Делчева обединяват усилия за реформа в психиатричната помощ", news.Title);
            Assert.Contains("Подобряването на психиатричната помощ, ролята на медицинските специалисти", news.Content);
            Assert.Contains("гарантират защита на правата и интересите на гражданите.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://www.mh.government.bg/upload/19809/", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 19), news.PostDate);
            Assert.Equal("aktualno/4813", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mh.government.bg/bg/novini/aktualno/4811";
            var provider = new MhGovernmentBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Ивкова: Финансовата дисциплина не означава съкращения и неизплащане на извънреден труд", news.Title);
            Assert.Contains("Указанията за финансова дисциплина, изпратени от Министерството на здравеопазването", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://www.mh.government.bg/upload/19800/", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 17), news.PostDate);
            Assert.Equal("aktualno/4811", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteEpidemicNewsShouldWorkCorrectly()
        {
            // Not the weekly morbili report (4721): the ministry rewrites that one in place, text and date.
            const string NewsUrl = "https://www.mh.government.bg/bg/novini/epidemichna-obstanovka/4645";
            var provider = new MhGovernmentBgEpidemicSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Справка за епидемичната обстановка в страната за периода 05.01.2026 г. – 11.01.2026 г.", news.Title);
            Assert.Contains("През изминалата седмица са регистрирани общо 6 073 случая на остри заразни заболявания", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2026, 1, 15), news.PostDate);
            Assert.Equal("epidemichna-obstanovka/4645", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MhGovernmentBgNewsSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public void GetEpidemicNewsShouldReturnResults()
        {
            var provider = new MhGovernmentBgEpidemicSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }

        [Fact(Skip = "mh.government.bg removed the 'ministerski-savet' news section in its 2026 redesign; this seeded sub-source is now obsolete and should be repointed to a current section or removed (a product decision).")]
        public void GetMinisterskiSuvetNewsShouldReturnResults()
        {
            var provider = new MhGovernmentBgMinisterskiSuvetSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public void GetParlamentarenKontrolNewsShouldReturnResults()
        {
            var provider = new MhGovernmentBgParlamentarenKontrolSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
