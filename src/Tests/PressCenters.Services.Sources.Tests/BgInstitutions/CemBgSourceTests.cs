namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CemBgSourceTests
    {
        [Theory]
        [InlineData("https://www.cem.bg/displaynewsbg/810", "810")]
        [InlineData("https://www.cem.bg/displaynewsbg/809/", "809")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CemBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.cem.bg/displaynewsbg/672";
            var provider = new CemBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Прессъобщение", news.Title);
            Assert.Equal("672", news.RemoteId);
            Assert.Equal(new DateTime(2020, 7, 10), news.PostDate);
            Assert.Contains("Съветът за електронни медии на 9 юли 2020 г. се срещна с посланика на САЩ", news.Content);
            Assert.Contains("предложение да споделят експертния потенциал на своите държави с България.", news.Content);
            Assert.Contains("https://www.cem.bg/files/images/IMG_2047.JPG", news.Content);
            Assert.DoesNotContain("Прессъобщение", news.Content);
            Assert.DoesNotContain("10 Юли 2020", news.Content);
            Assert.DoesNotContain("javascript", news.Content);
            Assert.DoesNotContain("IMG_2020", news.Content);
            Assert.Equal("https://www.cem.bg/files/images/IMG_2020 %281%29.JPG", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithoutAnImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.cem.bg/displaynewsbg/822";
            var provider = new CemBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Прессъобщение", news.Title);
            Assert.Equal("822", news.RemoteId);
            Assert.Equal(new DateTime(2022, 6, 9), news.PostDate.Date);
            Assert.Contains("По инициатива на Съвета за електронни медии се проведе среща с изпълнителния директор", news.Content);
            Assert.Contains("във връзка с прилагане на Закона за хазарта, в частта реклама на хазартни игри.", news.Content);
            Assert.DoesNotContain("Прессъобщение", news.Content);
            Assert.DoesNotContain("09 Юни 2022", news.Content);
            Assert.DoesNotContain("javascript", news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CemBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
