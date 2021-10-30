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
        [InlineData("https://toplo.bg/news/sravnitelnagrafika", "sravnitelnagrafika")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ToploBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://toplo.bg/news/2021/09/30/cokmladost";
            var provider = new ToploBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Центърът за обслужване на клиенти в кв. „Младост“ ще бъде временно затворен", news.Title);
            Assert.Equal("2021/09/30/cokmladost", news.RemoteId);
            Assert.Equal(new DateTime(2021, 9, 30), news.PostDate.Date);
            Assert.Contains("Може да посетите най-близкия Център за обслужване", news.Content);
            Assert.Contains("За допълнителна информация тел.: 0700 11 111", news.Content);
            Assert.DoesNotContain("closedmladost.jpg", news.Content);
            Assert.StartsWith("https://toplo.bg/media/новини/closedmladost.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://toplo.bg/news/2021/10/21/mladost";
            var provider = new ToploBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Отново отваря обновеният клиентски център в кв. „Младост“", news.Title);
            Assert.Equal("2021/10/21/mladost", news.RemoteId);
            Assert.Equal(new DateTime(2021, 10, 21), news.PostDate.Date);
            Assert.Contains("ви, че от 25 октомври", news.Content);
            Assert.Contains("да ни извинете за причиненото неудобство.", news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new ToploBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
