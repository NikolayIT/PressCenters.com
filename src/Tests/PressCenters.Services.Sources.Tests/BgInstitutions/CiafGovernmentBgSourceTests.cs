namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CiafGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("http://www.ciaf.government.bg/news/view/syobshtenie-08-09-2009-g-140/", "140")]
        [InlineData("http://www.ciaf.government.bg/news/view/pressyob-sht-enie-467/", "467")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CiafGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.ciaf.government.bg/news/view/kpkonpi-vnese-v-burgaski-ja-okryjen-syd-iskovite-molbiza-otnemane-na-imu-sht-estvo-sre-sht-u-nikolaj-i-evgeni-ja-banevi-468/";
            var provider = new CiafGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Кпконпи Внесе В Бургаския Окръжен Съд Исковите Молби За Отнемане На Имущество Срещу Николай И Евгения Баневи", news.Title);
            Assert.Equal("468", news.RemoteId);
            Assert.Equal(new DateTime(2020, 02, 03).Date, news.PostDate.Date);
            Assert.Contains("На свое заседание на 29 януари 2020 г. КПКОНПИ взе решение за предявяване", news.Content);
            Assert.Contains("с приложения и писмени доказателства от над 300 тома.", news.Content);
            Assert.DoesNotContain("thumb_820x460_5e37f42b02ed8.jpeg", news.Content);
            Assert.DoesNotContain("Връщане към списък", news.Content);
            Assert.DoesNotContain("Изпрати на e-mail", news.Content);
            Assert.Equal("http://www.ciaf.government.bg/web/attachments/News/468/3484/thumb_820x460_5e37f42b02ed8.jpeg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithoutImage()
        {
            const string NewsUrl = "http://www.ciaf.government.bg/news/view/pressyob-sht-enie-469/";
            var provider = new CiafGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Прессъобщение", news.Title);
            Assert.Equal("469", news.RemoteId);
            Assert.Equal(new DateTime(2020, 02, 10).Date, news.PostDate.Date);
            Assert.Contains("По повод съобщение до медиите на", news.Content);
            Assert.Contains("установяване на конфликт на интереси.", news.Content);
            Assert.Contains("Писмо от 27.05.2019", news.Content);
            Assert.DoesNotContain("Връщане към списък", news.Content);
            Assert.DoesNotContain("Изпрати на e-mail", news.Content);
            Assert.Equal("/images/sources/ciaf.government.bg.png", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CiafGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
