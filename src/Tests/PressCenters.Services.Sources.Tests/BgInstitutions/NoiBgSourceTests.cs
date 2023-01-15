namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class NoiBgSourceTests
    {
        [Theory]
        [InlineData("https://nssi.bg/nssi_sspf_meeting_10102022/", "nssi_sspf_meeting_10102022")]
        [InlineData("https://nssi.bg/udostoverenia-za-pro-shte-se-izdavatdo-21-dekemvri-2022", "udostoverenia-za-pro-shte-se-izdavatdo-21-dekemvri-2022")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new NoiBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://nssi.bg/sumata-za-nedostigasht-mesec-osiguritelen0staj0e-bez-promiana/";
            var provider = new NoiBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Сумата за недостигащ месец осигурителен стаж е без промяна – 140,58 лв.", news.Title);
            Assert.Contains("Без промяна от началото на 2023 г. остава сумата, която лицата внасят за така нареченото закупуване на осигурителен стаж", news.Content);
            Assert.Contains("е 3840, а средният недостигащ стаж – 22,18 месеца.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("pension_1.jpg", news.Content);
            Assert.DoesNotContain("януари 4, 2023", news.Content);
            Assert.Equal("https://nssi.bg/wp-content/uploads/pension_1.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2023, 1, 4, 10, 4, 9), news.PostDate);
            Assert.Equal("sumata-za-nedostigasht-mesec-osiguritelen0staj0e-bez-promiana", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new NoiBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
