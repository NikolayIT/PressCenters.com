namespace PressCenters.Services.Data.Tests
{
    using System;

    using PressCenters.Services.Data.Images;

    using Xunit;

    public class NewsImagePathsTests
    {
        [Theory]
        [InlineData(1, "news/1/big_1.webp", "news/1/small_1.webp")]
        [InlineData(1000, "news/0/big_1000.webp", "news/0/small_1000.webp")]
        [InlineData(123456, "news/456/big_123456.webp", "news/456/small_123456.webp")]
        public void KeysShouldMatchTheHistoricalFolderLayout(int newsId, string expectedBig, string expectedSmall)
        {
            Assert.Equal(expectedBig, NewsImagePaths.BigKey(newsId));
            Assert.Equal(expectedSmall, NewsImagePaths.SmallKey(newsId));
        }

        [Theory]
        [InlineData("/images", "/images/news/456/big_123456.webp")]
        [InlineData("/images/", "/images/news/456/big_123456.webp")]
        [InlineData("https://images.presscenters.com", "https://images.presscenters.com/news/456/big_123456.webp")]
        [InlineData("https://images.presscenters.com/", "https://images.presscenters.com/news/456/big_123456.webp")]
        public void UrlShouldJoinBaseAndKeyWithASingleSlash(string baseUrl, string expected)
        {
            Assert.Equal(expected, NewsImagePaths.Url(baseUrl, NewsImagePaths.BigKey(123456)));
        }

        [Theory]
        [InlineData("news/456/big_123456.png", "news/456/big_123456.webp")]
        [InlineData("news/0/small_1000.png", "news/0/small_1000.webp")]
        public void WebpKeyForPngShouldMapTheOldPngThumbnails(string pngKey, string expected)
        {
            Assert.Equal(expected, NewsImagePaths.WebpKeyForPng(pngKey));
        }

        [Theory]
        [InlineData("news/456/big_123456.webp")]
        [InlineData("news/custom/photo.png")]
        [InlineData("news/custom/big_1.jpg")]
        [InlineData("news/456/big_123456.png.tmp")]
        [InlineData("mainnews/1.png")]
        [InlineData(null)]
        public void WebpKeyForPngShouldIgnoreEverythingElse(string key)
        {
            Assert.Null(NewsImagePaths.WebpKeyForPng(key));
        }

        [Theory]
        [InlineData(null, "/images/mainnews/7.png?t=")]
        [InlineData("https://presscenters.com", "https://presscenters.com/images/mainnews/7.png?t=")]
        [InlineData("https://presscenters.com/", "https://presscenters.com/images/mainnews/7.png?t=")]
        public void MainNewsUrlShouldCarryTheStorysTime(string siteUrl, string expectedStart)
        {
            var createdOn = new DateTime(2026, 10, 25, 15, 34, 7);

            Assert.Equal(expectedStart + createdOn.ToBinary(), NewsImagePaths.MainNewsUrl(siteUrl, 7, createdOn));
        }

        [Fact]
        public void MainNewsUrlShouldChangeWithEveryNewStoryOfTheSource()
        {
            var createdOn = new DateTime(2026, 10, 25, 15, 34, 7);

            Assert.NotEqual(NewsImagePaths.MainNewsUrl(null, 7, createdOn), NewsImagePaths.MainNewsUrl(null, 7, createdOn.AddMinutes(20)));
        }

        [Fact]
        public void PngKeyForWebpShouldGiveTheOldPngThumbnail()
        {
            Assert.Equal("news/456/big_123456.png", NewsImagePaths.PngKeyForWebp(NewsImagePaths.BigKey(123456)));
            Assert.Equal("news/456/small_123456.png", NewsImagePaths.PngKeyForWebp(NewsImagePaths.SmallKey(123456)));
        }
    }
}
