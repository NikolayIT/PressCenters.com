namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class NapBgSourceTests
    {
        [Theory]
        [InlineData("https://old.nra.bg/news?id=3843", "3843")]
        [InlineData("https://old.nra.bg/news?id=337", "337")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new NapBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://old.nra.bg/news?id=3829";
            var provider = new NapBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Важно! Годишно приключване в НАП в периода 1-10 януари 2019 г.", news.Title);
            Assert.Contains("В периода от 1 до 10 януари 2019 г. в НАП ще се извършват дейности", news.Content);
            Assert.Contains("може да се получи на телефона на Информационния център на НАП 0700 18 700.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("03 Януари 2019", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 3), news.PostDate);
            Assert.Equal("3829", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new NapBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.Any());
        }
    }
}
