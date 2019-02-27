namespace PressCenters.Services.Sources.Tests.BgNgos
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgNgos;

    using Xunit;

    public class GallupInternationalBgSourceTests
    {
        [Theory]
        [InlineData("http://gallup-international.bg/bg/%D0%9F%D1%83%D0%B1%D0%BB%D0%B8%D0%BA%D0%B0%D1%86%D0%B8%D0%B8/85-2019/475-Express-Opinion-Poll-on-Current-Issues", "475")]
        [InlineData("http://gallup-international.bg/bg/%D0%9F%D1%83%D0%B1%D0%BB%D0%B8%D0%BA%D0%B0%D1%86%D0%B8%D0%B8/2011/141-438-%D0%BB%D0%B2-%D0%BC%D0%B8%D0%BD%D0%B8%D0%BC%D0%B0%D0%BB%D0%BD%D0%B0-%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%BD%D0%B0-%D0%B7%D0%B0%D0%BF%D0%BB%D0%B0%D1%82%D0%B0-%D0%B8%D1%81%D0%BA%D0%B0%D1%82-%D0%B1%D1%8A%D0%BB%D0%B3%D0%B0%D1%80%D0%B8%D1%82%D0%B5", "141")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new GallupInternationalBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.gallup-international.bg/bg/%D0%9F%D1%83%D0%B1%D0%BB%D0%B8%D0%BA%D0%B0%D1%86%D0%B8%D0%B8/1-2000/468-link";
            var provider = new GallupInternationalBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Експресен сондаж по актуални теми", news.Title);
            Assert.Contains("Обществото по-скоро съчувства на Елена Йончева в създалия се политически казус", news.Content);
            Assert.Contains("Gallup International Association or its members are not related to Gallup Inc.", news.Content);
            Assert.Contains("gallup-international.bg/images/2019/Ekspresen sondaj-Ioncheva/Picture3.png", news.Content);
            Assert.DoesNotContain("Следваща >", news.Content);
            Assert.DoesNotContain("01 Февруари 2019", news.Content);
            Assert.Equal("/images/sources/gallup-international.bg.png", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 2, 1, 9, 1, 0), news.PostDate);
            Assert.Equal("468", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new GallupInternationalBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
