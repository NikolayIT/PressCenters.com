namespace PressCenters.Services.Sources.Tests.MainNews
{
    using PressCenters.Services.Sources.MainNews;

    using Xunit;

    // Fast, deterministic (no-network) tests for the shared MakeAbsoluteUrl helper that every provider now
    // uses to normalise hrefs/srcs. Mixing absolute and relative links was the single most common cause of
    // silent provider breakage, so the resolution rules are pinned down here.
    public class BaseMainNewsProviderTests
    {
        [Theory]
        [InlineData("/news/article-1", "https://example.com/news/article-1")] // root-relative path
        [InlineData("article-2", "https://example.com/article-2")] // document-relative path
        [InlineData("https://cdn.example.org/img.jpg", "https://cdn.example.org/img.jpg")] // absolute kept as-is
        [InlineData("http://other.example/x", "http://other.example/x")] // absolute, different scheme/host
        [InlineData("//cdn.example.com/a.png", "https://cdn.example.com/a.png")] // protocol-relative -> base scheme
        [InlineData("  /trimmed  ", "https://example.com/trimmed")] // surrounding whitespace trimmed
        public void MakeAbsoluteUrlShouldResolveAgainstBaseUrl(string input, string expected)
        {
            var provider = new TestableMainNewsProvider();
            Assert.Equal(expected, provider.ResolveUrl(input));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MakeAbsoluteUrlShouldReturnNullForEmptyInput(string input)
        {
            var provider = new TestableMainNewsProvider();
            Assert.Null(provider.ResolveUrl(input));
        }

        // A minimal concrete provider that exposes the protected helper for testing.
        private class TestableMainNewsProvider : BaseMainNewsProvider
        {
            public override string BaseUrl => "https://example.com/";

            public override RemoteMainNews GetMainNews() => null;

            public string ResolveUrl(string url) => this.MakeAbsoluteUrl(url);
        }
    }
}
