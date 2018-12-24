namespace PressCenters.Services.Sources.Tests.MainNews
{
    using PressCenters.Services.Sources.MainNews;

    using Xunit;

    public class NovaBgMainNewsProviderTests
    {
        [Fact]
        public void GetMainNewsShouldWorkCorrectly()
        {
            var provider = new NovaBgMainNewsProvider();
            var news = provider.GetMainNews();
            Assert.NotNull(news.Title);
            Assert.True(news.Title.Length >= 10);
            Assert.Contains("nova.bg", news.OriginalUrl);
            Assert.StartsWith("http", news.OriginalUrl);
            Assert.StartsWith("http", news.ImageUrl);
        }
    }
}
