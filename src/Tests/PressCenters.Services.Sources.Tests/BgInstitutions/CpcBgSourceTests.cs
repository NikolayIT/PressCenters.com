namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CpcBgSourceTests
    {
        [Theory]
        [InlineData("https://www.cpc.bg/news-275?returnUrl=page%3d2", "275")]
        [InlineData("https://www.cpc.bg/news-273", "273")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CpcBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.cpc.bg/news-272";
            var provider = new CpcBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("КЗК активно следи пазарите на твърди горива в Република България", news.Title);
            Assert.Equal("272", news.RemoteId);
            Assert.Equal(new DateTime(2022, 9, 27), news.PostDate);
            Assert.Contains("връзка с наблюдаваното покачване на цените на твърди горива през последните две", news.Content);
            Assert.Contains("или на адрес: гр. София, бул. Витоша № 18.", news.Content);
            Assert.DoesNotContain("Назад", news.Content);
            Assert.DoesNotContain("09", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CpcBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
