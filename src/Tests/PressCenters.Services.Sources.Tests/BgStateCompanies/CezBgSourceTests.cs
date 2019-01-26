namespace PressCenters.Services.Sources.Tests.BgStateCompanies
{
    using System.Linq;

    using PressCenters.Services.Sources.BgStateCompanies;

    using Xunit;

    public class CezBgSourceTests
    {
        [Theory]
        [InlineData("http://www.cez.bg/bg/novini/38.html", "38")]
        [InlineData("http://www.cez.bg/bg/novini/1798.html", "1798")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CezBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.cez.bg/bg/novini/851.html";
            var provider = new CezBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("ЧЕЗ подари още 996,77 лева на свои лоялни клиенти", news.Title);
            Assert.Contains("Вече 550 клиенти на компанията получиха безплатни сметки за електричество", news.Content);
            Assert.Contains("zaklienta@cez.bg", news.Content);
            Assert.DoesNotContain("Новини", news.Content);
            Assert.DoesNotContain("Електронна фактура", news.Content);
            Assert.Equal("http://www.cez.bg/edee/content/img-other/bulgaria/cok-2.jpg", news.ImageUrl);
            //// Not supported: Assert.Equal(new DateTime(2019, 1, 17), news.PostDate);
            Assert.Equal("851", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.cez.bg/bg/novini/1799.html";
            var provider = new CezBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Блокиран е източникът на фалшиви имейли от името на ЧЕЗ Електро България", news.Title);
            Assert.Contains("Както информирахме по-рано днес, екипът на \"ЧЕЗ Електро България\" АД, ангажиран с киберсигурност", news.Content);
            Assert.Contains("Пресцентър на \"ЧЕЗ Електро България\" АД", news.Content);
            Assert.DoesNotContain("Новини", news.Content);
            Assert.DoesNotContain("Електронна фактура", news.Content);
            Assert.Equal("/images/sources/cez.bg.png", news.ImageUrl);
            //// Not supported: Assert.Equal(new DateTime(2019, 1, 17), news.PostDate);
            Assert.Equal("1799", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new CezBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
