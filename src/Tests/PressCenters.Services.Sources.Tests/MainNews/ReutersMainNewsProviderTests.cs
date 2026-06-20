namespace PressCenters.Services.Sources.Tests.MainNews
{
    using PressCenters.Services.Sources.MainNews;

    using Xunit;

    public class ReutersMainNewsProviderTests
    {
        [Fact(Skip = "reuters.com returns HTTP 401 to non-browser clients (hard anti-scraping block, fails even via curl); the provider is intentionally disabled in MainNewsSourcesSeeder.")]
        public void GetMainNewsShouldWorkCorrectly()
        {
            var provider = new ReutersMainNewsProvider();
            var news = provider.GetMainNews();
            Assert.NotNull(news.Title);
            Assert.True(news.Title.Length >= 10);
            Assert.Contains("reuters.com", news.OriginalUrl);
            Assert.StartsWith("https", news.OriginalUrl);
            Assert.StartsWith("https", news.ImageUrl);
        }
    }
}
