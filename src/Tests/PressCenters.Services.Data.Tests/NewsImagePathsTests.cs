namespace PressCenters.Services.Data.Tests
{
    using PressCenters.Services.Data.Images;

    using Xunit;

    public class NewsImagePathsTests
    {
        [Theory]
        [InlineData(1, "news/1/big_1.png", "news/1/small_1.png")]
        [InlineData(1000, "news/0/big_1000.png", "news/0/small_1000.png")]
        [InlineData(123456, "news/456/big_123456.png", "news/456/small_123456.png")]
        public void KeysShouldMatchTheHistoricalFolderLayout(int newsId, string expectedBig, string expectedSmall)
        {
            Assert.Equal(expectedBig, NewsImagePaths.BigKey(newsId));
            Assert.Equal(expectedSmall, NewsImagePaths.SmallKey(newsId));
        }

        [Theory]
        [InlineData("/images", "/images/news/456/big_123456.png")]
        [InlineData("/images/", "/images/news/456/big_123456.png")]
        [InlineData("https://images.presscenters.com", "https://images.presscenters.com/news/456/big_123456.png")]
        [InlineData("https://images.presscenters.com/", "https://images.presscenters.com/news/456/big_123456.png")]
        public void UrlShouldJoinBaseAndKeyWithASingleSlash(string baseUrl, string expected)
        {
            Assert.Equal(expected, NewsImagePaths.Url(baseUrl, NewsImagePaths.BigKey(123456)));
        }
    }
}
