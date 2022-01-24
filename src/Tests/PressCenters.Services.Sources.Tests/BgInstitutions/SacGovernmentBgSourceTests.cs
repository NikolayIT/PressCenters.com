namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;
    using Xunit;

    public class SacGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("http://www.sac.government.bg/news/bg/2021113-1", "2021113-1")]
        [InlineData("http://www.sac.government.bg/news/bg/2010331-0", "2010331-0")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new SacGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.sac.government.bg/news/bg/20131221-0";
            var provider = new SacGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Делегация от Върховния административен съд осъществи работно посещение във Върховния трибунал на Кралство Испания", news.Title);
            Assert.Contains("Върховният административен съд на Република България, в качеството си на бенефициент по проект", news.Content);
            Assert.Contains("В знак на благодарност, българската делегация връчи почетен плакет и алманах със 100 годишната история на Върховния административен съд на Република България.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("84a98da63f64ee144225814500483d3e/body/0.B9A", news.Content);
            Assert.Equal("http://www.sac.government.bg/home.nsf/9a2be833279bc3dcc2257beb0020cf53/84a98da63f64ee144225814500483d3e/body/0.B9A?OpenElement&FieldElemFormat=jpg", news.ImageUrl);
            Assert.Equal("20131221-0", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.sac.government.bg/news/bg/2021114-1";
            var provider = new SacGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Общото събрание на съдиите от Върховния касационен съд и Върховния административен съд избра Соня Янкулова за съдия в Конституционния съд на Република България", news.Title);
            Assert.Contains("Общото събрание на съдиите от Върховния касационен съд и Върховния административен съд, което се проведе днес, избра Соня Янкулова за съдия в Конституционния съд на Република България.", news.Content);
            Assert.Contains("Медиите имаха възможност да наблюдават директно процедурата по избора чрез видео стрийминг в интернет сайтовете на двете върховни съдилища.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal("2021114-1", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new SacGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
