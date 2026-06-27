namespace PressCenters.Services.Sources.Tests.MainNews
{
    using System;

    using PressCenters.Common;
    using PressCenters.Services.Sources.MainNews;

    using Xunit;

    // Data-driven smoke test over every provider that MainNewsSourcesSeeder actually seeds (the homepage
    // "top story" tiles). Each provider is resolved the same way the Hangfire MainNewsGetterJob resolves it
    // -- by fully-qualified type name via reflection -- so a typo in the seeder's TypeName, or a provider
    // whose markup has drifted, is caught here in one place instead of silently leaving a stale homepage tile.
    // Keep this list in sync with the non-commented entries in MainNewsSourcesSeeder.
    public class SeededMainNewsProvidersTests
    {
        [Theory]
        [InlineData("PressCenters.Services.Sources.MainNews.NewsBntBgMainNewsProvider", "bntnews.bg")]
        [InlineData("PressCenters.Services.Sources.MainNews.BtvNoviniteMainNewsProvider", "btvnovinite.bg")]
        [InlineData("PressCenters.Services.Sources.MainNews.NovaBgMainNewsProvider", "nova.bg")]
        [InlineData("PressCenters.Services.Sources.MainNews.DnesBgMainNewsProvider", "dnes.bg")]
        [InlineData("PressCenters.Services.Sources.MainNews.CnnMainNewsProvider", "cnn.com")]
        [InlineData("PressCenters.Services.Sources.MainNews.DnevnikBgMainNewsProvider", "dnevnik.bg")]
        [InlineData("PressCenters.Services.Sources.MainNews.EuronewsMainNewsProvider", "euronews.com")]
        [InlineData("PressCenters.Services.Sources.MainNews.BtaBgMainNewsProvider", "bta.bg")]
        [InlineData("PressCenters.Services.Sources.MainNews.MediapoolBgMainNewsProvider", "mediapool.bg")]
        [InlineData("PressCenters.Services.Sources.MainNews.ApMainNewsProvider", "apnews.com")]
        [InlineData("PressCenters.Services.Sources.MainNews.BnrBgMainNewsProvider", "bnr")]
        public void SeededProviderShouldReturnAValidMainNews(string typeName, string expectedHost)
        {
            var provider = ReflectionHelpers.GetInstance<BaseMainNewsProvider>(typeName);

            var news = provider.GetMainNews();

            Assert.NotNull(news);

            // Title: a real headline, not empty and not a tiny site label.
            Assert.False(string.IsNullOrWhiteSpace(news.Title), $"{typeName}: empty title.");
            Assert.True(news.Title.Trim().Length >= 10, $"{typeName}: title too short -- \"{news.Title}\".");

            // Original URL: an absolute, well-formed http(s) link on the expected site.
            Assert.True(
                Uri.TryCreate(news.OriginalUrl, UriKind.Absolute, out var originalUri),
                $"{typeName}: OriginalUrl is not absolute -- \"{news.OriginalUrl}\".");
            Assert.True(
                originalUri.Scheme == Uri.UriSchemeHttp || originalUri.Scheme == Uri.UriSchemeHttps,
                $"{typeName}: OriginalUrl is not http(s) -- \"{news.OriginalUrl}\".");
            Assert.Contains(expectedHost, originalUri.Host);

            // Image URL: an absolute, well-formed http(s) link. The host may differ from the article host
            // (a CDN, or the relay proxy for BTA), so only the URL shape is asserted here.
            Assert.True(
                Uri.TryCreate(news.ImageUrl, UriKind.Absolute, out var imageUri),
                $"{typeName}: ImageUrl is not absolute -- \"{news.ImageUrl}\".");
            Assert.True(
                imageUri.Scheme == Uri.UriSchemeHttp || imageUri.Scheme == Uri.UriSchemeHttps,
                $"{typeName}: ImageUrl is not http(s) -- \"{news.ImageUrl}\".");
            Assert.DoesNotContain("data:image", news.ImageUrl);
        }
    }
}
