namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MinFinBgSourceTests
    {
        [Theory]
        [InlineData("https://www.minfin.bg/bg/news/13412", "13412")]
        [InlineData("https://www.minfin.bg/bg/news/1/", "1")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MinFinBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.minfin.bg/bg/news/13412";
            var provider = new MinFinBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Заместник министър-председателят и министър на финансите Гълъб Донев подписа Спогодба за избягване на двойното данъчно облагане с Княжество Андора", news.Title);
            Assert.Contains("двойното данъчно облагане", news.Content);
            Assert.Equal("https://www.minfin.bg/upload/64316/ICO_9079.JPG", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 19), news.PostDate);
            Assert.Equal("13412", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MinFinBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.Any());
        }
    }
}
