namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class NsiBgNewsSourceTests
    {
        [Theory]
        [InlineData("https://www.nsi.bg/news/nsi-posreshtna-statistici-ot-turciya-9642", "9642")]
        [InlineData("https://www.nsi.bg/press-release/indeks-na-razhodite-za-trud-i-trimesechie-2026-godina-9338", "9338")]
        [InlineData("https://nsi.bg/bg/content/13854/somenewstitle/", "13854")]
        [InlineData("https://www.nsi.bg/bg/content/13840/somecategory/somenewstitle/", "13840")]
        public void ExtractIdFromNewsUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new NsiBgNewsSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.nsi.bg/news/nsi-posreshtna-statistici-ot-turciya-9642";
            var provider = new NsiBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("НСИ посрещна статистици от Турция", news.Title);
            Assert.Contains("Националният статистически институт прие представители на Статистическия институт на Република Турция", news.Content);
            Assert.Contains("необходими за изпълнението на европейските изисквания в областта на енергийната статистика.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://nsi.bg/uploads/files/News_News/", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 18, 10, 0, 0), news.PostDate);
            Assert.Equal("9642", news.RemoteId);
        }

        [Fact]
        public void ParseRemotePressNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.nsi.bg/press-release/indeks-na-razhodite-za-trud-i-trimesechie-2026-godina-9338";
            var provider = new NsiBgPressSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("Индекс на разходите за труд", news.Title);
            Assert.Contains("общите разходи на работодателите за един отработен час", news.Content);
            Assert.Contains("https://nsi.bg/file/36276/indeks_na_razhodite_za_trud.pdf", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 19, 11, 0, 0), news.PostDate);
            Assert.Equal("9338", news.RemoteId);
        }

        [Fact]
        public void ParseRemotePressNewsWithPdfShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.nsi.bg/press-release/ravnishta-na-potrebitelski-ceni-v-es-2025-godina-9366";
            var provider = new NsiBgPressSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("Равнища на потребителски цени в ЕС", news.Title);
            Assert.Contains("https://nsi.bg/file/36320/ravnishta_na_potrebitelski_ceni_v_es.pdf", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 18, 15, 10, 0), news.PostDate);
            Assert.Equal("9366", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new NsiBgNewsSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public void GetPressNewsShouldReturnResults()
        {
            var provider = new NsiBgPressSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
