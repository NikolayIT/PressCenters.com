namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class KzpBgSourceTests
    {
        [Theory]
        [InlineData("https://kzp.bg/bg/novini/416", "416")]
        [InlineData("https://kzp.bg/bg/novini/415/", "415")]
        [InlineData("https://kzp.bg/novini/zabraneni-sa-neloyalni-praktiki-na-telekom", "zabraneni-sa-neloyalni-praktiki-na-telekom")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new KzpBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithImage()
        {
            const string NewsUrl = "https://kzp.bg/bg/novini/415";
            var provider = new KzpBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("Дизайнът насочва потребителския избор", news.Title);
            Assert.Equal("415", news.RemoteId);
            Assert.Equal(new DateTime(2026, 6, 16), news.PostDate);
            Assert.Contains("Според ново проучване на EUIPO", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://kzp.bg/upload/156467/", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithDefaultImage()
        {
            const string NewsUrl = "https://kzp.bg/bg/novini/414";
            var provider = new KzpBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("КЗП установи нелоялни практики", news.Title);
            Assert.Equal("414", news.RemoteId);
            Assert.Equal(new DateTime(2026, 6, 12), news.PostDate);
            Assert.Contains("Комисията за защита на потребителите установи", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://kzp.bg/upload/155281/bez-snimka", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new KzpBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
