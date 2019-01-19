namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MfaBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mfa.bg/bg/news/20226", "20226")]
        [InlineData("https://www.mfa.bg/bg/news/53", "53")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MfaBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mfa.bg/bg/news/20209";
            var provider = new MfaBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Новоназначеният български посланик в Алжир връчи копия на акредитивните си писма", news.Title);
            Assert.Contains("На 26 декември т.г. новоназначеният извънреден", news.Content);
            Assert.Contains("техните възможности и на съществуващия потенциал.", news.Content);
            Assert.DoesNotContain("Новини", news.Content);
            Assert.DoesNotContain("Допълнителни снимки", news.Content);
            Assert.DoesNotContain("facebook.com", news.Content);
            Assert.DoesNotContain("upload/34436/DSC_6579.JPG", news.Content);
            Assert.DoesNotContain("28 Декември 2018", news.Content);
            Assert.Equal("https://www.mfa.bg/upload/34436/DSC_6579.JPG", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 28), news.PostDate);
            Assert.Equal("20209", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mfa.bg/bg/news/52";
            var provider = new MfaBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Среща на Николай Младенов с Бан Ки-мун", news.Title);
            Assert.Contains("България е в добра позиция да балансира положението на Балканите", news.Content);
            Assert.Contains("пострадалото от земетресение Хаити.", news.Content);
            Assert.DoesNotContain("Новини", news.Content);
            Assert.DoesNotContain("facebook.com", news.Content);
            Assert.DoesNotContain("04 Май 2010", news.Content);
            Assert.Equal("/images/sources/mfa.bg.png", news.ImageUrl);
            Assert.Equal(new DateTime(2010, 5, 4), news.PostDate);
            Assert.Equal("52", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MfaBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
