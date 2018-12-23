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
        [InlineData("http://toplo.bg/single-news/?id=32", "32")]
        [InlineData("http://toplo.bg/single-news/?id=17&asd=1", "17")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ToploBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://toplo.bg/single-news/?id=29";
            var provider = new ToploBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Възстановено топлоподаване в следните квартали:", news.Title);
            Assert.Equal("29", news.RemoteId);
            Assert.Null(news.ShortContent);
            Assert.Equal(new DateTime(2017, 2, 08, 8, 0, 0), news.PostDate);
            Assert.Contains("„Топлофикация София” ЕАД  уведомява  своите  клиенти", news.Content);
            Assert.Contains("ул. „Искърско шосе“, бул. „Цветан Лазаров“", news.Content);
            Assert.Contains("Пресцентър “Топлофикация  София” ЕАД", news.Content);
            Assert.Equal("/Content/Logos/toplo.bg.png", news.ImageUrl);
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
