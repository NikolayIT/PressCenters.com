namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class BfunionBgSourceTests
    {
        [Theory]
        [InlineData("https://bfunion.bg/news/51890/0", "51890")]
        [InlineData("https://bfunion.bg/news/46256/0", "46256")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new BfunionBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://bfunion.bg/news/51890/0";
            var provider = new BfunionBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Националните отбори за девойки U19 и U17 научиха съперниците си в първия кръг на европейските квалификации", news.Title);
            Assert.Contains("Националните отбори на България за девойки до 19 и до 17 години разбраха съперниците си", news.Content);
            Assert.Contains("Република Ирландия, Грузия и Армения", news.Content);
            Assert.Equal("https://bfunion.bg/uploads/2026-06-11/size1/image_1781187515_41333.jpeg", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 11, 17, 11, 0), news.PostDate);
            Assert.Equal("51890", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new BfunionBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
