namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MjsBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mjs.bg/117/4", "4")]
        [InlineData("https://www.mjs.bg/117/14880/", "14880")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MjsBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mjs.bg/117/14881/";
            var provider = new MjsBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Новата концепция за наказателна политика ще се изработи на базата на задълбочен анализ на прилагането на НК", news.Title);
            Assert.Contains("Необходимо е да се оцени прилагането на действащия Наказателен кодекс (НК),", news.Content);
            Assert.Contains("Министерството на правосъдието, ще бъдат използвани в изготвянето на концепцията.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("18.12.2018", news.Content);
            Assert.Equal(new DateTime(2018, 12, 18), news.PostDate);
            Assert.Equal("14881", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MjsBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
