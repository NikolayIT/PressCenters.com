namespace PressCenters.Services.Sources.Tests.BgStateCompanies
{
    using System.Linq;

    using PressCenters.Services.Sources.BgStateCompanies;

    using Xunit;

    public class PirogovEuSourceTests
    {
        [Theory]
        [InlineData("https://pirogov.eu/bg/v-pirogov-ima-patsient-s-imeto-staiko-staikov_p1749.html", "1749")]
        [InlineData("https://pirogov.eu/bg/news-1100_p1100.html", "1100")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new PirogovEuSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://pirogov.eu/bg/pirogov-otkri-i-detski-gripen-kabinet_p1747.html";
            var provider = new PirogovEuSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("\"Пирогов\" откри и детски грипен кабинет", news.Title);
            Assert.Contains("УМБАЛСМ \"Н. И. ПИРОГОВ\" откри&nbsp;и детски грипен кабинет. След като", news.Content);
            Assert.Contains("От там те ще бъдат насочени към грипния кабинет за възрастни.", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.DoesNotContain("fancybox", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("https://pirogov.eu/image_cache/f/9/6/e/7/f96e732895a56c80730cce4eff316a8625e4b99c.jpeg?v2", news.ImageUrl);
            //// Not supported: Assert.Equal(new DateTime(2019, 1, 28), news.PostDate);
            Assert.Equal("1747", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://pirogov.eu/bg/news-1102_p1102.html";
            var provider = new PirogovEuSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Стартира Курс по микрохирургия с международен лектор", news.Title);
            Assert.Contains("Световноизвестният специалист по микрохирургия проф. Едгар Бимер", news.Content);
            Assert.Contains("микрохирургичните дейности като присажадане на тъкани и органи.", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.DoesNotContain("fancybox", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("/images/sources/pirogov.eu.png", news.ImageUrl);
            //// Not supported: Assert.Equal(new DateTime(2012, 9, 29), news.PostDate);
            Assert.Equal("1102", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new PirogovEuSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
