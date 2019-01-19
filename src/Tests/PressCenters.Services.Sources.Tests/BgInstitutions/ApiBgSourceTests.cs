namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class ApiBgSourceTests
    {
        [Fact]
        public void ExtractIdFromUrlShouldWorkCorrectlyWithDashAtTheEnd()
        {
            var provider = new ApiBgSource();
            var result = provider.ExtractIdFromUrl("http://www.api.bg/index.php/bg/prescentar/novini/10-sa-kandidatite-za-izrabotvane-na-tehnicheski-proekt-pri-rehabilitaciyata-na-31-km-ot-pt-iii-861-ii-86-yugovo-lki-zdravec/");
            Assert.Equal("10-sa-kandidatite-za-izrabotvane-na-tehnicheski-proekt-pri-rehabilitaciyata-na-31-km-ot-pt-iii-861-ii-86-yugovo-lki-zdravec", result);
        }

        [Fact]
        public void ExtractIdFromUrlShouldWorkCorrectly()
        {
            var provider = new ApiBgSource();
            var result = provider.ExtractIdFromUrl("http://www.api.bg/index.php/bg/prescentar/novini/podpapka/druga-podpapka/filename");
            Assert.Equal("filename", result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.api.bg/bg/prescentar/novini/na-31-yanuari-izticha-validnostta-na-godishnite-vinetki-za-2015-g/";
            var provider = new ApiBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("На 31 януари изтича валидността на годишните винетки за 2015 г.", news.Title);
            Assert.Contains("На дирекциите „Социално подпомогане“ досега са предоставени безплатни", news.Content);
            Assert.Contains("Пълният списък на дистрибуторската мрежа е публикуван на интернет страницата на АПИ", news.Content);
            Assert.DoesNotContain("16e9043a49410f09048c7f65c06248bd_f4527.jpg", news.Content);
            Assert.Equal("http://www.api.bg/files/cache/16e9043a49410f09048c7f65c06248bd_f4527.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 24, 09, 59, 0), news.PostDate);
            Assert.Equal("na-31-yanuari-izticha-validnostta-na-godishnite-vinetki-za-2015-g", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithBigImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.api.bg/index.php/bg/prescentar/novini/zapochna-prodazhbata-na-e-vinetki-na-kasite-v-benzinostanciite-omv/";
            var provider = new ApiBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Започна продажбата на е-винетки на касите в бензиностанциите ОМВ", news.Title);
            Assert.Contains("Започна продажбата на електронни винетки на касите и в бензиностанциите ОМВ.", news.Content);
            Assert.Contains("До 8 часа тази сутрин са продадени 114 705 електронни винетки за 7 817 916 лв.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("09.01.2019 16:40", news.Content);
            Assert.DoesNotContain("Photo__vinetki_-_09.01.2019", news.Content);
            Assert.Equal("http://www.api.bg/files/3715/4704/5587/Photo__vinetki_-_09.01.2019.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 9, 16, 40, 0), news.PostDate);
            Assert.Equal("zapochna-prodazhbata-na-e-vinetki-na-kasite-v-benzinostanciite-omv", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithOneOfTheFirstNews()
        {
            const string NewsUrl = "http://www.api.bg/index.php/bg/prescentar/novini/8-firmi-podadoha-oferti-za-stroitelstvoto-na-lot-2-ot-am-trakiya/";
            var provider = new ApiBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("8 фирми подадоха оферти за строителството на Лот 2 от АМ „Тракия“", news.Title);
            Assert.Contains("„Днешното събитие е плод на един огромен и сериозен труд на всички служители в агенцията”", news.Content);
            Assert.Contains("националния бюджет чрез Оперативна програма „Транспорт“ 2007-2013 г.", news.Content);
            Assert.DoesNotContain("Logo-OPT.png", news.Content);
            Assert.Equal("http://www.api.bg/files/3113/6830/4295/Logo-OPT.png", news.ImageUrl);
            Assert.Equal(new DateTime(2010, 1, 12, 14, 31, 0), news.PostDate);
            Assert.Equal("8-firmi-podadoha-oferti-za-stroitelstvoto-na-lot-2-ot-am-trakiya", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new ApiBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
