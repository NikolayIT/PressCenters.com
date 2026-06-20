namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class ModBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mod.bg/news9386", "9386")]
        [InlineData("https://www.mod.bg/news938", "938")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ModBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mod.bg/news9386";
            var provider = new ModBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("Министърът на отбраната Димитър Стоянов представи в Брюксел", news.Title);
            Assert.Equal("9386", news.RemoteId);
            Assert.Equal(new DateTime(2026, 6, 18), news.PostDate);
            Assert.Contains("ще продължи да изпълнява ангажиментите си към НАТО", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://www.mod.bg/uploads/news/", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mod.bg/news938";
            var provider = new ModBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("Заместник-министърът на отбраната Августина Цветкова изнесе лекция", news.Title);
            Assert.Equal("938", news.RemoteId);
            Assert.Equal(new DateTime(2012, 9, 21), news.PostDate);
            Assert.Contains("участието на жените в структурите на сигурността и отбраната е жизнено необходимо", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new ModBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
