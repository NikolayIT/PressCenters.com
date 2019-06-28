namespace PressCenters.Services.Sources.Tests.BgStateCompanies
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgStateCompanies;

    using Xunit;

    public class ToploBgSourceTests
    {
        [Theory]
        [InlineData("https://toplo.bg/news/2018/12/13/smetkinoemvri", "2018/12/13/smetkinoemvri")]
        [InlineData("https://toplo.bg/news/2018/11/06/puskameparnoto", "2018/11/06/puskameparnoto")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ToploBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://toplo.bg/news/2019/06/24/1-11";
            var provider = new ToploBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Профилактика в кв. „Младост\" 1, кв. „Мусагеница\", кв. „Студентски град\", кв. „Дървеница\" и кв.Витоша", news.Title);
            Assert.Equal("2019/06/24/1-11", news.RemoteId);
            Assert.Equal(new DateTime(2019, 6, 24), news.PostDate.Date);
            Assert.Contains("„Топлофикация София” ЕАД съобщава на своите клиенти, че", news.Content);
            Assert.Contains("хотел „Вега“, офис сграда „Трелеборг“,НХА", news.Content);
            Assert.Contains("“Топлофикация София” ЕАД поднася своите извинения на засегнатите клиенти за причиненото неудобство и разчита на тяхното разбиране.", news.Content);
            Assert.DoesNotContain("blog/URBAN_WORN.png", news.Content);
            Assert.Equal("https://toplo.bg/assets/images/blog/URBAN_WORN.png", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new ToploBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
