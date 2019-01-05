namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MlspBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mlsp.government.bg/index.php?section=PRESS2&prid=1576&lang=", "1576")]
        [InlineData("https://www.mlsp.government.bg/index.php?section=PRESS2&prid=1572", "1572")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MlspBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mlsp.government.bg/index.php?section=PRESS2&prid=1575";
            var provider = new MlspBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Над 25 500 души ще получават услугите „Личен асистент“,  „Социален асистент“ и „Домашен помощник“ и през 2019 г.", news.Title);
            Assert.Contains("Със Закона за държавния бюджет на Република България за 2019", news.Content);
            Assert.Contains("без прекъсване в предоставянето на необходимата подкрепа.", news.Content);
            Assert.DoesNotContain("mtsp_rsz%20(1).jpg", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Equal("https://www.mlsp.government.bg/server/php/files/1111 (4).jpg", news.ImageUrl);
            Assert.Equal("1575", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mlsp.government.bg/index.php?section=PRESS2&prid=87&lang=";
            var provider = new MlspBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Над 12 300 безработни са започнали в реалната икономика в област Варна", news.Title);
            Assert.Contains("За посочения период в бюрата по труда са заявени общо 10107 места", news.Content);
            Assert.Contains("трудовите правоотношения и здравословните и безопасни условия на труд.", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Equal("https://www.mlsp.government.bg/server/php/files/mtsp_rsz (1).jpg", news.ImageUrl);
            Assert.Equal("87", news.RemoteId);
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
