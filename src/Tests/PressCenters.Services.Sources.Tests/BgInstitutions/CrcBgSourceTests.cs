namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CrcBgSourceTests
    {
        [Theory]
        [InlineData("https://crc.bg/bg/novini/1465/pozicii-na-konsultativnite-saveti-po-vaprosite-za-sigurnostta-na-mrejite-i-uslugite", "1465")]
        [InlineData("https://crc.bg/bg/novini/14/komisijata-za-regulirane-na-syobshtenijata-odobri-predlojenite-ot-bylgarskata-telekomunikacionna-kompanija-ad-obshti-uslovija-na-dogovora-za-polzvaneto-na-podzemnata-kabelna-mreja", "14")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CrcBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithImage()
        {
            const string NewsUrl = "https://crc.bg/bg/novini/1402/krs-s-nova-mobilna-stanciq-za-radiomonitoring-i-izmervaniq";
            var provider = new CrcBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("КРС с нова мобилна станция за радиомониторинг и измервания", news.Title);
            Assert.Equal("1402", news.RemoteId);
            Assert.Equal(new DateTime(2021, 7, 20, 17, 35, 0), news.PostDate);
            Assert.Contains("С мобилната станция ще се извършва по-добър мониторинг и контрол на радиочестотния спектър, включително и на 5G мрежите", news.Content);
            Assert.Contains("конкуренцията при осъществяване на електронни съобщения и гарантиране ефективното управление и използване на радиочестотния спектър.", news.Content);
            Assert.DoesNotContain("files", news.Content);
            Assert.DoesNotContain("20/07/21", news.Content);
            Assert.Equal("https://crc.bg/files/ВОП/41 s.png", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithDefaultImage()
        {
            const string NewsUrl = "https://crc.bg/bg/novini/1473/%D0%9A%D0%BE%D0%BC%D0%B8%D1%81%D0%B8%D1%8F%D1%82%D0%B0%20%D0%B7%D0%B0%20%D1%80%D0%B5%D0%B3%D1%83%D0%BB%D0%B8%D1%80%D0%B0%D0%BD%D0%B5%20%D0%BD%D0%B0%20%D1%81%D1%8A%D0%BE%D0%B1%D1%89%D0%B5%D0%BD%D0%B8%D1%8F%D1%82%D0%B0%20%D0%B2%D0%B7%D0%B5%20%D1%83%D1%87%D0%B0%D1%81%D1%82%D0%B8%D0%B5%20%D0%B2%20%D0%9C%D0%B8%D0%BD%D0%B8%D1%81%D1%82%D0%B5%D1%80%D1%81%D0%BA%D0%B0%D1%82%D0%B0%20%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%B0%20%D0%BD%D0%B0%20%D0%A1%D0%B2%D0%B5%D1%82%D0%BE%D0%B2%D0%BD%D0%B8%D1%8F%20%D0%BC%D0%BE%D0%B1%D0%B8%D0%BB%D0%B5%D0%BD%20%D0%BA%D0%BE%D0%BD%D0%B3%D1%80%D0%B5%D1%81%20%D0%B2%20%D0%91%D0%B0%D1%80%D1%81%D0%B5%D0%BB%D0%BE%D0%BD%D0%B0";
            var provider = new CrcBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Комисията за регулиране на съобщенията взе участие в Министерската програма на Световния мобилен конгрес в Барселона", news.Title);
            Assert.Equal("1473", news.RemoteId);
            Assert.Equal(new DateTime(2022, 3, 9, 0, 0, 0), news.PostDate.Date);
            Assert.Contains("Комисията за регулиране на съобщенията (КРС), представлявана от нейните членове", news.Content);
            Assert.Contains("предизвикателствата и възможностите по пътя към изпълнение на целите на цифровото десетилетие.", news.Content);
            Assert.DoesNotContain("news-1.jpg", news.Content);
            Assert.DoesNotContain("09/03/22", news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CrcBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
