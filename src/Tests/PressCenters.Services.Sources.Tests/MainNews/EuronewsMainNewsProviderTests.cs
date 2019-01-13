namespace PressCenters.Services.Sources.Tests.MainNews
{
    using PressCenters.Services.Sources.MainNews;

    using Xunit;

    public class EuronewsMainNewsProviderTests
    {
        [Fact]
        public void GetMainNewsShouldWorkCorrectly()
        {
            var provider = new EuronewsMainNewsProvider();
            var news = provider.GetMainNews();
            Assert.NotNull(news.Title);
            Assert.True(news.Title.Length >= 10);
            Assert.Contains("euronews.com", news.OriginalUrl);
            Assert.StartsWith("https://", news.OriginalUrl);
            Assert.StartsWith("https://", news.ImageUrl);
        }
    }
}
