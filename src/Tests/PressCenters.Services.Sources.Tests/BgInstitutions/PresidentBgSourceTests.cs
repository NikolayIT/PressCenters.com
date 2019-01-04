namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class PresidentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.president.bg/news3575/prezidentat-rumen-radev-otnosheniyata-s-germaniya-sa-ot-strategichesko-znachenie-za-balgariya.html", "3575")]
        [InlineData("https://www.president.bg/news3567/prezidentat-rumen-radev-balgariya-razchita-na-nato-kato-garant-za-sigurnostta-na-darzhavite-chlenki.html", "3567")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new PresidentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.president.bg/news3566/rumen-radev-sigurnostta-na-evropeyskite-grazhdani-zapochva-ot-sigurnostta-na-granitsite-na-balgariya.html";
            var provider = new PresidentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Румен Радев: Сигурността на европейските граждани започва от сигурността на границите на България", news.Title);
            Assert.Equal("3566", news.RemoteId);
            Assert.Equal(new DateTime(2017, 1, 30, 21, 11, 0), news.PostDate);
            Assert.Contains("Сигурността на европейските граждани започва от гарантирането", news.Content);
            Assert.Contains("Европейския парламент за предприетите", news.Content);
            Assert.Contains("Обединените въоръжени сили на НАТО в Европа, в Монс.", news.Content);
            Assert.DoesNotContain("Румен Радев: Сигурността на европейските граждани започва от сигурността на границите на България", news.Content);
            Assert.DoesNotContain("30 Януари 2017 | 21:11", news.Content);
            Assert.Equal("https://www.president.bg/images/news/mm_1485803640.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.president.bg/news4597/darzhavniyat-glava-rumen-radev-ministar-predsedatelyat-i-resorniyat-ministar-tryabva-da-organizirat-saglasuvaneto-na-predlozhenieto-za-osvobozhdavane-na-glavniya-sekretar-na-mvr.html";
            var provider = new PresidentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Държавният глава Румен Радев:  Министър-председателят и ресорният министър трябва да организират съгласуването на предложението за освобождаване на главния секретар на МВР", news.Title);
            Assert.Equal("4597", news.RemoteId);
            Assert.Equal(new DateTime(2018, 9, 17, 18, 0, 0), news.PostDate);
            Assert.Contains("„Поканен съм на среща", news.Content);
            Assert.Contains("„Кабинетът си има министър-председател", news.Content);
            Assert.Contains("допълни държавният глава.", news.Content);
            Assert.DoesNotContain("Държавният глава Румен Радев: Министър-председателят и ресорният министър трябва", news.Content);
            Assert.DoesNotContain("17 Септември 2018 | 18:00", news.Content);
            Assert.Equal("/images/sources/president.bg.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new PresidentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(17, result.Count());
        }
    }
}
