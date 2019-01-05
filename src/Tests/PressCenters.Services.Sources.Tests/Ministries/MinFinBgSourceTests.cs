namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MinFinBgSourceTests
    {
        [Theory]
        [InlineData("https://www.minfin.bg/bg/news/10540", "10540")]
        [InlineData("https://www.minfin.bg/bg/news/1/", "1")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MinFinBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.minfin.bg/bg/news/10188";
            var provider = new MinFinBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министърът На Финансите Владислав Горанов Застана Начело На Съвета Екофин За Първите Шест Месеца На 2018 Г.", news.Title);
            Assert.Contains("Министърът на финансите Владислав Горанов председателства заседанието на Съвета на ЕС", news.Content);
            Assert.Contains("Изявлението на министър Горанов пред медиите", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.DoesNotContain("date", news.Content);
            Assert.DoesNotContain(".jpg", news.Content);
            Assert.Equal("https://www.minfin.bg/upload/36335/1.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 1, 23), news.PostDate);
            Assert.Equal("10188", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.minfin.bg/bg/news/10538";
            var provider = new MinFinBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Мф Очаква Излишък В Размер На 163,5 Млн. Лв.  По Консолидираната Фискална Програма За 2018 Г.", news.Title);
            Assert.Contains("На база на предварителни данни и оценки се очаква", news.Content);
            Assert.Contains("на интернет страницата на Министерството на финансите в края на месец януари 2019 година.", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.DoesNotContain("date", news.Content);
            Assert.Equal("/images/sources/minfin.bg.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 31), news.PostDate);
            Assert.Equal("10538", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MinFinBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.Any());
        }
    }
}
