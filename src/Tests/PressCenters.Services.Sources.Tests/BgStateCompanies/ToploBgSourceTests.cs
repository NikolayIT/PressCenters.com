namespace PressCenters.Services.Sources.Tests.BgStateCompanies
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources;
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
            const string NewsUrl = "https://toplo.bg/news/2018/12/17/kolednaigra2018";
            var provider = new ToploBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Коледна игра 2018", news.Title);
            Assert.Equal("2018/12/17/kolednaigra2018", news.RemoteId);
            Assert.Null(news.ShortContent);
            Assert.Equal(new DateTime(2018, 12, 17), news.PostDate.Date);
            Assert.Contains("Днес, 17.12.2018, в навечерието на коледните и новогодишни празници стартираме традиционната ни", news.Content);
            Assert.Contains("Организаторът на „Коледната игра“ не е отговорен", news.Content);
            Assert.Contains("или на фейсбук страницата", news.Content);
            Assert.DoesNotContain("images/blog/1200x350.png", news.Content);
            Assert.Equal("https://toplo.bg/assets/images/blog/1200x350.png", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new ToploBgSource();
            var result = provider.GetLatestPublications(new LocalPublicationsInfo { LastLocalId = null });
            Assert.True(result.News.Any());
        }
    }
}
