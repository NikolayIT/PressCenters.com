namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class McGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("http://mc.government.bg/newsn.php?n=6674&i=1", "6674")]
        [InlineData("http://mc.government.bg/newsn.php?n=6671", "6671")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new McGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://mc.government.bg/newsn.php?n=42&i=1";
            var provider = new McGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министърът На Културата Проф. Стефан Данаилов Откри Обновената Зала “Средец” С Изложбата От Макети На Култови Сгради “България На Длан”", news.Title);
            Assert.Contains("Днес, 3 януари 2006 г., министърът на културата проф. Стефан Данаилов ", news.Content);
            Assert.Contains("осигуряването на постоянна експозиционна площ за изложбата “България на длан”.", news.Content);
            Assert.Contains("http://mc.government.bg/images/page-0080_9.jpg", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("03.01.2006", news.Content);
            Assert.DoesNotContain("42_article-0080.jpg", news.Content);
            Assert.Equal(new DateTime(2006, 1, 3), news.PostDate);
            Assert.Equal("http://mc.government.bg/news/42_article-0080.jpg", news.ImageUrl);
            Assert.Equal("42", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://mc.government.bg/newsn.php?n=39&i=1";
            var provider = new McGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министърът На Културата Проф. Стефан Данаилов Е Награден С Ордена \"Звезда На Италианската Солидарност\"", news.Title);
            Assert.Contains("Президентът на Италия Карло Адзелио Чампи издаде указ за награждаване ", news.Content);
            Assert.Contains("Джанфранко Фини по повод Празника на националния трибагреник.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("09.01.2006", news.Content);
            Assert.Equal(new DateTime(2006, 1, 9), news.PostDate);
            Assert.Null(news.ImageUrl);
            Assert.Equal("39", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new McGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
