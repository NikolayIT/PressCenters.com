namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MlspGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mlsp.government.bg/inspektsiyata-po-truda-napomnya-na-rabotodatelite-da-doplnyat-otsenkata-na-riska-vv-vrzka-s-epidemiologichnata-obstanovka", "inspektsiyata-po-truda-napomnya-na-rabotodatelite-da-doplnyat-otsenkata-na-riska-vv-vrzka-s-epidemiologichnata-obstanovka")]
        [InlineData("https://www.mlsp.government.bg/ministr-denitsa-sacheva-prefokusirame-politikata-po-zaetost-km-povishavane-na-kachestvoto-na-rabotna-sila", "ministr-denitsa-sacheva-prefokusirame-politikata-po-zaetost-km-povishavane-na-kachestvoto-na-rabotna-sila")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MlspBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mlsp.government.bg/blizo-220-000-sluzhiteli-shche-zapazyat-rabotnite-si-mesta-po-myarkata-6040";
            var provider = new MlspBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Близо 220 000 Служители Ще Запазят Работните Си Места По Мярката 60/40", news.Title);
            Assert.Contains("Работните места на 219 845 служители в 13 497 предприятия ще бъдат", news.Content);
            Assert.Contains("на 21 472 работници от близо 1000 предприятия.", news.Content);
            Assert.DoesNotContain("jobs.jpeg", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Equal(new DateTime(2020, 5, 20), news.PostDate);
            Assert.Equal("https://www.mlsp.government.bg/uploads/4/snimki-za-novini-brekzit/jobs.jpeg", news.ImageUrl);
            Assert.Equal("blizo-220-000-sluzhiteli-shche-zapazyat-rabotnite-si-mesta-po-myarkata-6040", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly2()
        {
            const string NewsUrl = "https://www.mlsp.government.bg/s-darenie-ot-ban-oshche-157-detsa-ot-tsnst-shche-poluchat-tableti-za-distantsionno-obuchenie";
            var provider = new MlspBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("С Дарение От Бан: Още 157 Деца От Цнст Ще Получат Таблети За Дистанционно Обучение", news.Title);
            Assert.Contains("Още 157 деца от Центрове за настаняване от семеен тип ще получат персонални таблети", news.Content);
            Assert.Contains("Центровете за обществена подкрепа и Центровете за социална интеграция и рехабилитация.", news.Content);
            Assert.DoesNotContain("homework-3235100-1280.jpg", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Equal(new DateTime(2020, 5, 20), news.PostDate);
            Assert.Equal("https://www.mlsp.government.bg/uploads/43/homework-3235100-1280.jpg", news.ImageUrl);
            Assert.Equal("s-darenie-ot-ban-oshche-157-detsa-ot-tsnst-shche-poluchat-tableti-za-distantsionno-obuchenie", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MlspBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
