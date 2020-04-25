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
            const string NewsUrl = "https://ime.bg/bg/articles/2019-denyat-na-danyna-svoboda-idva-na-18-mai/";
            var provider = new ImeBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("2019: Денят на данъчна свобода идва на 18 май", news.Title);
            Assert.Contains("Преразпределението през бюджета е най-високо за последните 10 години", news.Content);
            Assert.Contains("където всеки ще може да изчисли своята лична данъчна тежест.", news.Content);
            Assert.DoesNotContain("Оцени тази статия", news.Content);
            Assert.DoesNotContain("04.01.2019", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 4), news.PostDate);
            Assert.Equal("2019-denyat-na-danyna-svoboda-idva-na-18-mai", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithAuthorShouldWorkCorrectly()
        {
            const string NewsUrl = "https://ime.bg/bg/articles/surva-surva-godina-silna-ikonomika-dogodina/";
            var provider = new ImeBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Сурва, сурва година, силна икономика догодина", news.Title);
            Assert.Contains("Повечето анализатори ще се съгласят, че изпратихме една добра година за икономиката на страната", news.Content);
            Assert.Contains("слънчева, макар и с малко повече облачност.", news.Content);
            Assert.DoesNotContain("Оцени тази статия", news.Content);
            Assert.DoesNotContain("Десислава Николова", news.Content);
            Assert.DoesNotContain("04.01.2019", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 4), news.PostDate);
            Assert.Equal("surva-surva-godina-silna-ikonomika-dogodina", news.RemoteId);
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
