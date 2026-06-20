namespace PressCenters.Services.Sources.Tests.BgStateCompanies
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgStateCompanies;

    using Xunit;

    public class IsBgNetSourceTests
    {
        [Theory]
        [InlineData("https://is-bg.net/bg/publications/news/544", "544")]
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
            const string NewsUrl = "https://is-bg.net/bg/publications/news/544";
            var provider = new IsBgNetSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Информационно обслужване и Google Cloud стартират AI-базирана национална киберзащита в България", news.Title);
            Assert.Contains("Националният системен интегратор на България Информационно обслужване", news.Content);
            Assert.DoesNotContain("ПРЕДИШНА НОВИНА", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://is-bg.net/upload/", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 5, 20), news.PostDate);
            Assert.Equal("544", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://is-bg.net/bg/publications/news/546";
            var provider = new IsBgNetSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("Интервю на изпълнителния директор на Информационно обслужване Ивайло Филипов", news.Title);
            Assert.Contains("наскоро разказа как Гърция стигна от фалит до отличник на ЕС", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://is-bg.net/upload/", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 5, 22), news.PostDate);
            Assert.Equal("546", news.RemoteId);
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
