namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MonBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mon.bg/news/133-godishno-chitalisthe-vav-velingrad/", "133-godishno-chitalisthe-vav-velingrad")]
        [InlineData("https://www.mon.bg/news/balgarski-ucheniczi-specheliha-4-medala/", "balgarski-ucheniczi-specheliha-4-medala")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MonBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mon.bg/news/133-godishno-chitalisthe-vav-velingrad-pokazva-che-ucheneto-ne-spira-s-vazrastta/";
            var provider = new MonBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("133-Годишно Читалище Във Велинград Показва, Че Ученето Не Спира С Възрастта", news.Title);
            Assert.Contains("Народно читалище", news.Content);
            Assert.Contains("Велинград", news.Content);
            Assert.Equal("https://www.mon.bg/nfs/2026/06/viber_image_2026-06-17_11-30-35-320.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 18), news.PostDate);
            Assert.Equal("133-godishno-chitalisthe-vav-velingrad-pokazva-che-ucheneto-ne-spira-s-vazrastta", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MonBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
