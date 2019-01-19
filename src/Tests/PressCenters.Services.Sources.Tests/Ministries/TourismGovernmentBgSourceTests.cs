namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class TourismGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("http://www.tourism.government.bg/bg/kategorii/novini/ministur-angelkova-vsichki-uslugi-na-ministerstvoto-veche-sa-dostupni-s-edin-klik", "novini/ministur-angelkova-vsichki-uslugi-na-ministerstvoto-veche-sa-dostupni-s-edin-klik")]
        [InlineData("http://www.tourism.government.bg/bg/kategorii/novini/nad-8423-mln-sa-vizitite-ot-chuzhdenci-v-bulgariya-za-10-te-meseca-na-2018-g/", "novini/nad-8423-mln-sa-vizitite-ot-chuzhdenci-v-bulgariya-za-10-te-meseca-na-2018-g")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new TourismGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.tourism.government.bg/bg/kategorii/novini/ministur-angelkova-osnoven-akcent-v-rabotata-ni-prez-2019-g-shte-e-nasurchavane-na";
            var provider = new TourismGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Ангелкова: Основен акцент в работата ни през 2019 г. ще е насърчаване на вътрешния туризъм и привличането на инвестиции", news.Title);
            Assert.Contains("С майстори в кулинарията подготвяме карта на фестивалите,", news.Content);
            Assert.Contains("всички участващи, заключи министърът на туризма.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("15-12-18-darik_radio", news.Content);
            Assert.DoesNotContain("15 декември 2018", news.Content);
            Assert.DoesNotContain("facebook.com", news.Content);
            Assert.DoesNotContain("print", news.Content);
            Assert.Equal("http://www.tourism.government.bg/sites/tourism.government.bg/files/15-12-18-darik_radio.jpeg", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 15, 13, 51, 0), news.PostDate);
            Assert.Equal("novini/ministur-angelkova-osnoven-akcent-v-rabotata-ni-prez-2019-g-shte-e-nasurchavane-na", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.tourism.government.bg/bg/kategorii/novini/ministur-angelkova-s-registur-na-zabelezhitelnostite-shte-privlichame-poveche";
            var provider = new TourismGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Ангелкова: С регистър на забележителностите ще привличаме повече туристи", news.Title);
            Assert.Contains("С бъдещия регистър на туристическите обекти, атракции и забележителности", news.Content);
            Assert.Contains("азиатските туристически пазари, допълни още министър Ангелкова.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("28 ноември 2014", news.Content);
            Assert.DoesNotContain("facebook.com", news.Content);
            Assert.DoesNotContain("print", news.Content);
            Assert.Equal("/images/sources/tourism.government.bg.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2014, 11, 28, 10, 31, 0), news.PostDate);
            Assert.Equal("novini/ministur-angelkova-s-registur-na-zabelezhitelnostite-shte-privlichame-poveche", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new TourismGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
