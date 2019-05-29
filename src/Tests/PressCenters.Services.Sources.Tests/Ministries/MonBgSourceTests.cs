namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MonBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mon.bg/bg/news/3438", "3438")]
        [InlineData("https://www.mon.bg/bg/news/508", "508")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MonBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mon.bg/bg/news/3437";
            var provider = new MonBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Учениците Излизат В 12-Дневна Ваканция", news.Title);
            Assert.Contains("Учениците в цялата страна ще почиват 12 дни по Коледа и Нова година.", news.Content);
            Assert.Contains("държавните зрелостни изпити – 21 и 23 май 2019 г.", news.Content);
            Assert.DoesNotContain("soc-facebook", news.Content);
            Assert.DoesNotContain("facebook.com", news.Content);
            Assert.DoesNotContain("upload/18378/3008_ub2fc.jpg", news.Content);
            Assert.DoesNotContain("21.12.2018", news.Content);
            Assert.Equal("https://www.mon.bg/upload/18378/3008_ub2fc.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 21), news.PostDate);
            Assert.Equal("3437", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mon.bg/bg/news/578";
            var provider = new MonBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Клисарова Поздрави Катедрата По Български Език В Университета „Йотвьош Лоранд“ В Будапеща", news.Title);
            Assert.Contains("Министърът на образованието и науката проф. Анелия Клисарова", news.Content);
            Assert.Contains("България, Унгария, Чехия, Германия, Австрия, Русия, Румъния, Словения.", news.Content);
            Assert.DoesNotContain("soc-facebook", news.Content);
            Assert.DoesNotContain("facebook.com", news.Content);
            Assert.DoesNotContain("28.04.2014", news.Content);
            Assert.Equal("/images/sources/mon.bg.png", news.ImageUrl);
            Assert.Equal(new DateTime(2014, 4, 28), news.PostDate);
            Assert.Equal("578", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MonBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
