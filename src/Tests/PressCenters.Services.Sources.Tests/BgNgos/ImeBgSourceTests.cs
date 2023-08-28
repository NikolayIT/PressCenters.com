namespace PressCenters.Services.Sources.Tests.BgNgos
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgNgos;

    using Xunit;

    public class ImeBgSourceTests
    {
        [Theory]
        [InlineData("https://ime.bg/bg/articles/rekordnite-plashtaniya-na-byudjeta-prez-dekemvri-2018/", "rekordnite-plashtaniya-na-byudjeta-prez-dekemvri-2018")]
        [InlineData("https://ime.bg/bg/articles/softuernite-firmi-smqna-na-paradigmata-na-zastypnichestwoto-1", "softuernite-firmi-smqna-na-paradigmata-na-zastypnichestwoto-1")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ImeBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://ime.bg/articles/2019-denyat-na-danyna-svoboda-idva-na-18-mai/";
            var provider = new ImeBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("2019: Денят на данъчна свобода идва на 18 май", news.Title);
            Assert.Contains("Преразпределението през бюджета е най-високо за последните 10 години", news.Content);
            Assert.Contains("където всеки ще може да изчисли своята лична данъчна тежест.", news.Content);
            Assert.DoesNotContain("Оцени тази статия", news.Content);
            Assert.DoesNotContain("04-01-2019", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 4), news.PostDate);
            Assert.Equal("2019-denyat-na-danyna-svoboda-idva-na-18-mai", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithImage()
        {
            const string NewsUrl = "https://ime.bg/articles/kolko-byrzo-se-topi-inflatsiya-istoricheski-analogii/";
            var provider = new ImeBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Колко бързо се „топи“ инфлация: исторически аналогии", news.Title);
            Assert.Contains("Данните за юли показват упоритостта на инфлацията както в еврозоната, така и в България.", news.Content);
            Assert.Contains("По статията работи и Никол Вълканова, стажант в ИПИ", news.Content);
            Assert.DoesNotContain("25-08-2023", news.Content);
            Assert.Equal("https://ime.bg/wp-content/uploads/2023/08/shop_111-915x290.png", news.ImageUrl);
            Assert.Equal(new DateTime(2023, 8, 25), news.PostDate);
            Assert.Equal("kolko-byrzo-se-topi-inflatsiya-istoricheski-analogii", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new ImeBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.Any());
        }
    }
}
