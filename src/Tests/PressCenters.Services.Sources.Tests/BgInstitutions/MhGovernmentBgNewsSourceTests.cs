namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class MhGovernmentBgNewsSourceTests
    {
        [Theory]
        [InlineData("http://www.mh.government.bg/bg/novini/epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-strana16-03", "epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-strana16-03")]
        [InlineData("http://www.mh.government.bg/bg/novini/aktualno/komisiyata-po-izgotvyane-na-nacionalna-zdravna-kar/", "aktualno/komisiyata-po-izgotvyane-na-nacionalna-zdravna-kar")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MhGovernmentBgNewsSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.mh.government.bg/bg/novini/aktualno/stanovishe-na-pacientski-organizacii-zaedno-s-teb-/";
            var provider = new MhGovernmentBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Становище на пациентски организации „Заедно с теб“ относно ваксината „Пентаксим“", news.Title);
            Assert.Contains("Пациентски организации „Заедно с теб“", news.Content);
            Assert.Contains("Становище на пациентски организации „Заедно с теб“ относно ваксината „Пентаксим“", news.Content);
            Assert.Equal("/images/sources/mh.government.bg.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 22, 9, 49, 2), news.PostDate);
            Assert.Equal("aktualno/stanovishe-na-pacientski-organizacii-zaedno-s-teb-", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.mh.government.bg/bg/novini/aktualno/kiril-ananiev-prvite-dve-linejki-zakupeni-po-proek/";
            var provider = new MhGovernmentBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Кирил Ананиев: Първите две линейки, закупени по проекта за модернизация на спешната помощ, са предназначени за ЦСМП София", news.Title);
            Assert.Contains("„Първите две линейки, закупени", news.Content);
            Assert.Contains("този документ отразява официалното становище на Европейския съюз и Управляващия орган на ОПРР 2014-2020 г", news.Content);
            Assert.DoesNotContain("http://www.mh.government.bg/media/filer_public_thumbnails/filer_public/2018/12/20/priemane-lineiki-20-12-2018-1.jpg__623x416_q85_crop_subsampling-2_upscale.jpg“", news.Content);
            Assert.Equal("http://www.mh.government.bg/media/filer_public_thumbnails/filer_public/2018/12/20/priemane-lineiki-20-12-2018-1.jpg__623x416_q85_crop_subsampling-2_upscale.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 20, 13, 55, 51), news.PostDate);
            Assert.Equal("aktualno/kiril-ananiev-prvite-dve-linejki-zakupeni-po-proek", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteEpidemicNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.mh.government.bg/bg/novini/epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-st-2016-01/";
            var provider = new MhGovernmentBgEpidemicSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Справка за епидемичната обстановка в страната за периода 04.12.2015 г. - 10.01.2016 г.", news.Title);
            Assert.Contains("През изминалата седмица са регистрирани общо 6", news.Content);
            Assert.Contains("През изминалата седмица не са регистрирани хранителни взривове и взривове от вътреболнични инфекции.", news.Content);
            Assert.Equal("/images/sources/mh.government.bg.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 15, 15, 44, 0), news.PostDate);
            Assert.Equal("epidemichna-obstanovka/spravka-za-epidemichnata-obstanovka-v-st-2016-01", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MhGovernmentBgNewsSource();
            var result = provider.GetLatestPublications();

            Assert.True(result.Count() >= 10);
        }

        [Fact]
        public void GetEpidemicNewsShouldReturnResults()
        {
            var provider = new MhGovernmentBgEpidemicSource();
            var result = provider.GetLatestPublications();

            Assert.True(result.Count() >= 10);
        }
    }
}
