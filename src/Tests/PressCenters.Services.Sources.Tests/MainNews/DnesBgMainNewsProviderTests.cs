namespace PressCenters.Services.Sources.Tests.MainNews
{
    using PressCenters.Services.Sources.MainNews;

    using Xunit;

    public class DnesBgMainNewsProviderTests
    {
        [Fact]
        public void GetMainNewsShouldWorkCorrectly()
        {
            var provider = new DnesBgMainNewsProvider();
            var news = provider.GetMainNews();
            Assert.NotNull(news.Title);
            Assert.True(news.Title.Length >= 10);
            Assert.Contains("dnes.bg", news.OriginalUrl);
            Assert.StartsWith("http", news.OriginalUrl);
            Assert.StartsWith("http", news.ImageUrl);
        }
    }
}
