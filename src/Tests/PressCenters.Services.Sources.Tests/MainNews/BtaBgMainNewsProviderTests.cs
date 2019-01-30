namespace PressCenters.Services.Sources.Tests.MainNews
{
    using PressCenters.Services.Sources.MainNews;

    using Xunit;

    public class BtaBgMainNewsProviderTests
    {
        [Fact]
        public void GetMainNewsShouldWorkCorrectly()
        {
            var provider = new BtaBgMainNewsProvider();
            var news = provider.GetMainNews();
            Assert.NotNull(news.Title);
            Assert.DoesNotContain("...", news.Title);
            Assert.True(news.Title.Length >= 10);
            Assert.Contains("bta.bg", news.OriginalUrl);
        }
    }
}
