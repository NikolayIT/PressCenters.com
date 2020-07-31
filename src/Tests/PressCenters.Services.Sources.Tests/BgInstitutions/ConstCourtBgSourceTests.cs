namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class ConstCourtBgSourceTests
    {
        [Theory]
        [InlineData("http://www.constcourt.bg/bg/Blog/Display/888?type=1", "888")]
        [InlineData("http://www.constcourt.bg/bg/Blog/Display/369", "369")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ConstCourtBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.constcourt.bg/bg/Blog/Display/889?type=1";
            var provider = new ConstCourtBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Конституционният съд се произнесе с решение по конституционно дело № 1/2020 г.", news.Title);
            Assert.Equal("889", news.RemoteId);
            Assert.Equal(new DateTime(2020, 7, 30), news.PostDate.Date);
            Assert.Contains("Днес, 30.07.2020 г., Конституционният съд се произнесе с тълкувателно решение по конституционно дело № 1/2020 г.", news.Content);
            Assert.Contains("Други действия по разследването могат да бъдат извършвани без ограничение.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
            Assert.DoesNotContain("30 юли 2020 г.", news.Content);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new ConstCourtBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
