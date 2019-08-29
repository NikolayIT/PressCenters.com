namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CpdpBgSourceTests
    {
        [Theory]
        [InlineData("https://www.cpdp.bg/index.php?p=news_view&aid=1505", "1505")]
        [InlineData("https://www.cpdp.bg/index.php?p=news_view&aid=1381", "1381")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CpdpBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithImage()
        {
            const string NewsUrl = "https://www.cpdp.bg/index.php?p=news_view&aid=1463";
            var provider = new CpdpBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Проведено събитие по проект „e-OpenSpace”", news.Title);
            Assert.Equal("1463", news.RemoteId);
            Assert.Equal(new DateTime(2019, 8, 2), news.PostDate.Date);
            Assert.Contains("На   31 юли 2019 г. (сряда) от 10.00 ч. в Хотел ИнтерКонтинентал София се   проведе събитие за разпространение на резултатите по проект", news.Content);
            Assert.Contains("Комисията за защита на личните данни е координатор на проекта.", news.Content);
            Assert.DoesNotContain("02.08.2019", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("принт", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.Equal("https://www.cpdp.bg/pic/news/big/1463.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithoutImage()
        {
            const string NewsUrl = "https://www.cpdp.bg/index.php?p=news_view&aid=1519";
            var provider = new CpdpBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Информация за извършена проверка в Националната агенция за приходите", news.Title);
            Assert.Equal("1519", news.RemoteId);
            Assert.Equal(new DateTime(2019, 8, 29), news.PostDate.Date);
            Assert.Contains("В  хода на извършена в срок от един месец проверка на Националната агенция  за приходите (НАП)", news.Content);
            Assert.Contains("За целта не е необходимо да се иска  официален документ от КЗЛД или произнасяне на Комисията по конкретен  случай.", news.Content);
            Assert.DoesNotContain("29.08.2019", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("принт", news.Content);
            Assert.Equal("/images/sources/cpdp.bg.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CpdpBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
