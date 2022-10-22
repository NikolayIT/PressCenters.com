namespace PressCenters.Services.Sources.Tests.BgStateCompanies
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgStateCompanies;

    using Xunit;

    public class IsBgNetSourceTests
    {
        [Theory]
        [InlineData("https://www.is-bg.net/bg/news/194", "194")]
        [InlineData("https://is-bg.net/bg/news/304/", "304")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new IsBgNetSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://is-bg.net/bg/news/306";
            var provider = new IsBgNetSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Мобилното приложение „еЗдраве“ осигурява достъп на потребителите до личното им пациентско досие", news.Title);
            Assert.Contains("Всички граждани вече имат достъп през мобилно устройство до личното си пациентско досие и до всички налични електронни здравни документи", news.Content);
            Assert.Contains("за издаване и управление на квалифицирани удостоверения за електронни подписи, електронни печати и квалифицирани електронни времеви печати.", news.Content);
            Assert.DoesNotContain("ПРЕДИШНА НОВИНА", news.Content);
            Assert.DoesNotContain("29 Септември, 2022", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("https://www.is-bg.net/upload/3148/zdrave.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2022, 9, 29), news.PostDate);
            Assert.Equal("306", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithGenericImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://is-bg.net/bg/news/277";
            var provider = new IsBgNetSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Позиция на \"Информационно обслужване\" АД по повод обвиненията на управителя на НЗОК г-н Петко Салчев", news.Title);
            Assert.Contains("Във връзка необоснованите твърдения на управителя на НЗОК г-н Петко Салчев пред парламентарната Комисия по електронно управление и информационни технологии", news.Content);
            Assert.Contains("удостоверения за електронни подписи, електронни печати и квалифицирани електронни времеви печати.", news.Content);
            Assert.DoesNotContain("ПРЕДИШНА НОВИНА", news.Content);
            Assert.DoesNotContain("26 Май, 2022", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("https://www.is-bg.net/upload/2864/io-generic.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2022, 5, 26), news.PostDate);
            Assert.Equal("277", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new IsBgNetSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
