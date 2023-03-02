namespace PressCenters.Services.Sources.Tests.BgStateCompanies
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgStateCompanies;

    using Xunit;

    public class CezBgSourceTests
    {
        [Theory]
        [InlineData("https://electrohold.bg/bg/mediya-centr-group/novini/chez-elektro-s-nov-internet-adres-cezelectrobg/", "chez-elektro-s-nov-internet-adres-cezelectrobg")]
        [InlineData("https://electrohold.bg/bg/mediya-centr-group/novini/stroitelna-firma-ostavi-bez-tok-800-klienti-na-chez-razpredelenie-v-studentski-grad/", "stroitelna-firma-ostavi-bez-tok-800-klienti-na-chez-razpredelenie-v-studentski-grad")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CezBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://electrohold.bg/bg/mediya-centr-group/novini/elektrohold-she-e-novoto-ime-na-chez-v-blgariya/";
            var provider = new CezBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Електрохолд ще е новото име на ЧЕЗ в България", news.Title);
            Assert.Contains("Електрохолд ще е новото име на дружествата на ЧЕЗ в България от края на април 2022 г.", news.Content);
            Assert.Contains("Смяната на имената и логата не налага клиентите да предприемат никакви допълнителни действия.", news.Content);
            Assert.DoesNotContain("10 март 2022", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("https://electrohold.bg/media/images/vision_CEZ_Eurohold.0977fca4.fill-1358x420-c100.png", news.ImageUrl);
            Assert.Equal(new DateTime(2022, 3, 10), news.PostDate);
            Assert.Equal("elektrohold-she-e-novoto-ime-na-chez-v-blgariya", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithDefaultImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://electrohold.bg/bg/mediya-centr-group/novini/chez-vazstanovi-zahranvaneto-na-vsichki-selishta-v-obshtina-lovech/";
            var provider = new CezBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("ЧЕЗ възстанови захранването на всички селища в община Ловеч", news.Title);
            Assert.Contains("Възстановено е захранването на всички селища от община Ловеч", news.Content);
            Assert.Contains("адрес за по-бързо локализиране на засегнатите участъци в населените места.", news.Content);
            Assert.DoesNotContain("08 февруари 2020", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("https://electrohold.bg/media/images/Building_CEZ_2021_2.2e16d0ba.fill-1358x420-c100.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2020, 2, 8), news.PostDate);
            Assert.Equal("chez-vazstanovi-zahranvaneto-na-vsichki-selishta-v-obshtina-lovech", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new CezBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
