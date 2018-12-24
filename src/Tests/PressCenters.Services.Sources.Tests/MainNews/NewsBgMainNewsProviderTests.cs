namespace PressCenters.Services.Sources.Tests.MainNews
{
    using PressCenters.Services.Sources.MainNews;

    using Xunit;

    public class NewsBgMainNewsProviderTests
    {
        [Fact]
        public void GetMainNewsShouldWorkCorrectly()
        {
            var provider = new NewsBgMainNewsProvider();
            var news = provider.GetMainNews();
            Assert.NotNull(news.Title);
            Assert.True(news.Title.Length >= 10);
            Assert.Contains("news.bg", news.OriginalUrl);
            Assert.StartsWith("http", news.OriginalUrl);
            Assert.StartsWith("http", news.ImageUrl);
        }
    }
}
