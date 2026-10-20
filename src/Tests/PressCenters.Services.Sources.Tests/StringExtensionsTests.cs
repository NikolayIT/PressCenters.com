namespace PressCenters.Services.Sources.Tests
{
    using PressCenters.Common;

    using Xunit;

    public class StringExtensionsTests
    {
        [Fact]
        public void StripHtmlShouldRemoveTagsWithLineBreaksInAttributes()
        {
            var result = "<p>Text <a title=\"first\nsecond\" href=\"/x\">link</a> end</p>".StripHtml();

            Assert.Equal("Text link end", result);
        }

        [Fact]
        public void GetLastStringBetweenShouldReturnTheDefaultValueWhenTheEndIsMissing()
        {
            const string Url = "https://example.com/news/123";

            Assert.Equal(Url, Url.GetLastStringBetween("/", ".html", Url));
            Assert.Equal("slug", "https://example.com/news/slug.html".GetLastStringBetween("/", ".html", Url));
        }
    }
}
