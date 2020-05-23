namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MrrbBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mrrb.bg/bg/lyulin-stava-chast-ot-struma-putyat-kalotina-sofiya-stava-magistrala-evropa-82343/", "lyulin-stava-chast-ot-struma-putyat-kalotina-sofiya-stava-magistrala-evropa-82343")]
        [InlineData("https://www.mrrb.bg/bg/2880-razresheniya-za-polzvane-na-infrastrukturni-obekti-v-stranata-sa-izdadeni-ot-dnsk-ot-nachaloto-na-godinata", "2880-razresheniya-za-polzvane-na-infrastrukturni-obekti-v-stranata-sa-izdadeni-ot-dnsk-ot-nachaloto-na-godinata")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MrrbBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mrrb.bg/bg/ministur-avramova-ste-razporedi-proverka-na-uchastucite-ot-republikanskata-putna-mreja-s-vuvedeni-ogranicheniya-na-skorostta/";
            var provider = new MrrbBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Аврамова ще разпореди проверка на участъците от републиканската пътна мрежа с въведени ограничения на скоростта", news.Title);
            Assert.Contains("Ще разпоредя да бъдат обследвани участъци от републиканската пътна мрежа", news.Content);
            Assert.Contains("че българските лаборатории са също европейски независими лаборатории, които имат лицензи.", news.Content);
            Assert.Equal("https://www.mrrb.bg/static/media/ups/cached/10d5e52da29dbf10e75533e84cb733f7a90a36fc.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 22, 18, 51, 0), news.PostDate);
            Assert.Equal("ministur-avramova-ste-razporedi-proverka-na-uchastucite-ot-republikanskata-putna-mreja-s-vuvedeni-ogranicheniya-na-skorostta", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mrrb.bg/bg/na-26-09-2003-g-glavna-direkciya-grajdanska-registraciya-i-administrativno-obslujvane-kum-mrrb-osiguri-web-dostup-do-izbiratelnite-spisuci-za-mestni-izbori-2003-za-vsichki-bulgarski-grajdani/";
            var provider = new MrrbBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("На 26.09.2003 г. Главна Дирекция “Гражданска регистрация и административно обслужване” към МРРБ осигури Web достъп до избирателните списъци за Местни избори - 2003 за всички български граждани", news.Title);
            Assert.Contains("Гражданите ще могат да проверят своите лични данни", news.Content);
            Assert.Contains("От 1990 г. всички избирателни списъци се отпечатват от ГРАО.", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2007, 11, 4, 23, 12, 0), news.PostDate);
            Assert.Equal("na-26-09-2003-g-glavna-direkciya-grajdanska-registraciya-i-administrativno-obslujvane-kum-mrrb-osiguri-web-dostup-do-izbiratelnite-spisuci-za-mestni-izbori-2003-za-vsichki-bulgarski-grajdani", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MrrbBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
