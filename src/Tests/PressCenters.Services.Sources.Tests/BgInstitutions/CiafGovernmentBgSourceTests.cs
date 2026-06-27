namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CiafGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://ciaf.bg/bg/aktualno/novini/konpi-postigna-uspeh-po-tri-dela-za-otnemane-na-nezakonno-pridobito-imu-sht-estvo", "konpi-postigna-uspeh-po-tri-dela-za-otnemane-na-nezakonno-pridobito-imu-sht-estvo")]
        [InlineData("https://ciaf.bg/bg/aktualno/novini/na-vnimanieto-na-chlenovete-na-cik/", "na-vnimanieto-na-chlenovete-na-cik")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CiafGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://ciaf.bg/bg/aktualno/novini/nov-bylgarski-universitet-i-komisijata-za-otnemane-na-nezakonno-pridobitoto-imushtestvo-podpisaha-memorandum-za-sytrudnichestvo";
            var provider = new CiafGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Нов български университет и Комисията за отнемане на незаконно придобитото имущество подписаха меморандум за сътрудничество", news.Title);
            Assert.Equal("nov-bylgarski-universitet-i-komisijata-za-otnemane-na-nezakonno-pridobitoto-imushtestvo-podpisaha-memorandum-za-sytrudnichestvo", news.RemoteId);
            Assert.Equal(new DateTime(2026, 6, 19).Date, news.PostDate.Date);
            Assert.Contains("Днес Нов български университет (НБУ) и Комисията за отнемане на незаконно придобитото имущество (КОНПИ) подписаха Меморандум за сътрудничество", news.Content);
            Assert.Contains("реализация на студентите в областта на правото и публичните политики.", news.Content);
            Assert.Equal("https://ciaf.bg/web/files/news/687/main_image/thumb_1008x437_20260619_134922.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithoutImage()
        {
            const string NewsUrl = "https://ciaf.bg/bg/aktualno/novini/komisijata-za-otnemane-na-nezakonno-pridobitoto-imushtestvo-provede-onlajn-rabotna-sreshta-s-predstaviteli-na-otdela-za-izpylnenie-na-reshenijata-na-esp-ch-kym-syveta-na-evropa";
            var provider = new CiafGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Комисията за отнемане на незаконно придобитото имущество проведе онлайн работна среща с представители на Отдела за изпълнение на решенията на ЕСПЧ към Съвета на Европа", news.Title);
            Assert.Equal(new DateTime(2026, 6, 26).Date, news.PostDate.Date);
            Assert.Contains("Днес Комисията за отнемане на незаконно придобитото имущество проведе онлайн работна среща", news.Content);
            Assert.Equal("https://ciaf.bg/web/frontend/images/placeholder/placeholder-1008x437.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CiafGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
