namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class ApiBgSourceTests
    {
        [Theory]
        [InlineData("https://api.bg/bg/1636554345.html", "1636554345")]
        [InlineData("https://api.bg/bg/1610264152.html", "1610264152")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var source = new ApiBgSource();
            var result = source.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://api.bg/bg/1636184416.html";
            var provider = new ApiBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("В неделя за автомобилно състезание за няколко часа ще бъде ограничено движението по пътя Стойките - Смолян и Пампорово - Смолян", news.Title);
            Assert.Contains("Утре - 7 ноември, от 8:30 ч. до 11:30 ч., ще бъде ограничено движението по участък", news.Content);
            Assert.Contains("който събира и обобщава данните за състоянието на републиканските пътища.", news.Content);
            Assert.DoesNotContain("5a3ce4098d782730d035723466112c41", news.Content);
            Assert.DoesNotContain("06.11.2021", news.Content);
            Assert.DoesNotContain("В неделя за автомобилно състезание", news.Content);
            Assert.Equal("https://api.bg/files/imagecache/5a3ce4098d782730d035723466112c41_256x144.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2021, 11, 6, 9, 38, 0), news.PostDate);
            Assert.Equal("1636184416", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithOneOfTheFirstNews()
        {
            const string NewsUrl = "https://api.bg/bg/1610264152.html";
            var provider = new ApiBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("8 фирми подадоха оферти за строителството на Лот 2 от АМ „Тракия“", news.Title);
            Assert.Contains("„Днешното събитие е плод на един огромен и сериозен труд на всички служители в агенцията”", news.Content);
            Assert.Contains("националния бюджет чрез Оперативна програма „Транспорт“ 2007-2013 г.", news.Content);
            Assert.DoesNotContain("1fc5c10fd4f3b3baf46527f5a4965d57", news.Content);
            Assert.DoesNotContain("12.01.2010", news.Content);
            Assert.DoesNotContain("8 фирми подадоха оферти", news.Content);
            Assert.Equal("https://api.bg/files/imagecache/1fc5c10fd4f3b3baf46527f5a4965d57_256x144.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2010, 1, 12, 14, 31, 0), news.PostDate);
            Assert.Equal("1610264152", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new ApiBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
