namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class McGovernmentBgSourceTests
    {
        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://mc.government.bg/новини/министър-евтим-милошев-обяви-в-пловди/";
            var provider = new McGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.StartsWith("Министър Евтим Милошев обяви в Пловдив носителя на голямата награда", news.Title);
            Assert.Contains("Министърът на културата обяви, че ще предложи Зала 1 на НДК да носи името", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("https://mc.government.bg/wp-content/uploads/2026/06/6-1.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 12), news.PostDate);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new McGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(4, result.Count());
        }
    }
}
