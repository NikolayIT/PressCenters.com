namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MtitcGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mtitc.government.bg/bg/category/1/ministur-rosen-zhelyazkov-zasilvame-kontrola-po-vreme-na-praznicite", "1/ministur-rosen-zhelyazkov-zasilvame-kontrola-po-vreme-na-praznicite")]
        [InlineData("https://www.mtitc.government.bg/bg/category/1/otkrivane-na-noviya-trenazhoren-kompleks-za-podgotovka-na-rukovoditeli-na-poleti-na-dp-rukovodstvo-na-vuzdushnoto-dvizhenie/", "1/otkrivane-na-noviya-trenazhoren-kompleks-za-podgotovka-na-rukovoditeli-na-poleti-na-dp-rukovodstvo-na-vuzdushnoto-dvizhenie")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MtitcGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mtitc.government.bg/bg/category/1/rosen-zhelyazkov-shte-zashtitim-nacionalnite-interesi-s-vsichki-vuzmozhni-pohvati-pri-glasuvaneto-na-paketa-za-mobilnost-i";
            var provider = new MtitcGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Росен Желязков: Ще защитим националните интереси с всички възможни похвати при гласуването на Пакета за мобилност I", news.Title);
            Assert.Contains("Трябва да бъдат използвани всички похвати на европейската бюрокрация, за да защитим националните интереси", news.Content);
            Assert.Contains("ще присъства на предстоящия протест на българските превозвачи в Брюксел.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("1-1_125.jpg", news.Content);
            Assert.DoesNotContain("04.01.2019", news.Content);
            Assert.DoesNotContain("facebook.com", news.Content);
            Assert.DoesNotContain("gallery", news.Content);
            Assert.Equal("https://www.mtitc.government.bg/sites/default/files/1-1_130.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 4, 17, 44, 0), news.PostDate);
            Assert.Equal("1/rosen-zhelyazkov-shte-zashtitim-nacionalnite-interesi-s-vsichki-vuzmozhni-pohvati-pri-glasuvaneto-na-paketa-za-mobilnost-i", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MtitcGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(20, result.Count());
        }
    }
}
