namespace PressCenters.Services.Sources.Tests.BgStateCompanies
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgStateCompanies;

    using Xunit;

    public class SofiyskaVodaBgSourceTests
    {
        [Theory]
        [InlineData("https://www.sofiyskavoda.bg/novini/sofiyska-voda-vavezhda-za-parvi-pat-v-balgariya-digitalen-onlayn-monitoring-na-vik-mrezhata", "sofiyska-voda-vavezhda-za-parvi-pat-v-balgariya-digitalen-onlayn-monitoring-na-vik-mrezhata")]
        [InlineData("https://sofiyskavoda.bg/novini/fransoa-deberg-regionalen-direktor-na-veoliya-za-balgariya-v-intervyu-za-v-kapital-sofiyskata-prechistvatelna-stanciya-e-primer-za-kragova-ikonomika", "fransoa-deberg-regionalen-direktor-na-veoliya-za-balgariya-v-intervyu-za-v-kapital-sofiyskata-prechistvatelna-stanciya-e-primer-za-kragova-ikonomika")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new SofiyskaVodaBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.sofiyskavoda.bg/novini/vtoro-myasto-za-sofiya-v-indeksa-za-optimalno-polzvane-na-vodata-na-economist-impact";
            var provider = new SofiyskaVodaBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Второ място за София в Индекса за оптимално ползване на водата на Economist Impact", news.Title);
            Assert.Equal("vtoro-myasto-za-sofiya-v-indeksa-za-optimalno-polzvane-na-vodata-na-economist-impact", news.RemoteId);
            Assert.Equal(new DateTime(2022, 1, 13), news.PostDate.Date);
            Assert.Contains("София е на второ място за Западна и Източна Европа и на шесто място", news.Content);
            Assert.Contains("регистрира консолидирани приходи от 26.010 млрд. евро през 2020 г.", news.Content);
            Assert.DoesNotContain("Photo.PNG", news.Content);
            Assert.DoesNotContain("13.01.2022", news.Content);
            Assert.StartsWith("https://www.sofiyskavoda.bg/modules/news/Снимки Новини/Economist Index Photo.PNG", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithDefaultImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.sofiyskavoda.bg/novini/sofiyska-voda-s-merki-sreshtu-uslozhnenata-gripna-obstanovka";
            var provider = new SofiyskaVodaBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("„Софийска вода“ с мерки срещу усложнената грипна обстановка", news.Title);
            Assert.Equal("sofiyska-voda-s-merki-sreshtu-uslozhnenata-gripna-obstanovka", news.RemoteId);
            Assert.Equal(new DateTime(2020, 3, 5), news.PostDate.Date);
            Assert.Contains("Във връзка с регистрираните случаи на корона вирус в столицата", news.Content);
            Assert.Contains("нашите служители, за които носим отговорност.", news.Content);
            Assert.DoesNotContain(".jpg", news.Content);
            Assert.DoesNotContain("05.03.2020", news.Content);
            Assert.StartsWith("https://www.sofiyskavoda.bg/modules/news/Снимки Новини/Съобщение.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new SofiyskaVodaBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
