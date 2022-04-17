namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class NapBgSourceTests
    {
        [Theory]
        [InlineData("https://nra.bg/wps/portal/nra/actualno/NAP-nabludava-firmi-proizvoditeli-na-qica-i-agneshko", "NAP-nabludava-firmi-proizvoditeli-na-qica-i-agneshko")]
        [InlineData("https://nra.bg/wps/portal/nra/actualno/Po-leki-antikovid-merki-v-ofisite-na-NAP", "Po-leki-antikovid-merki-v-ofisite-na-NAP")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new NapBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://nra.bg/wps/portal/nra/actualno/NAP_zaporira_stoka_na_firma_ukrila_100_hil_lv_DDS";
            var provider = new NapBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("НАП запорира стока на фирма, укрила 100 хил. лв. ДДС", news.Title);
            Assert.Contains("Национална агенция за приходите наложи запор върху стоки, собственост на фирма, укрила близо 100 хил. лв.", news.Content);
            Assert.Contains("инициатива на НАП фирмата е с прекратена регистрация по Закона за ДДС.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal("NAP_zaporira_stoka_na_firma_ukrila_100_hil_lv_DDS", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new NapBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.Any());
        }
    }
}
