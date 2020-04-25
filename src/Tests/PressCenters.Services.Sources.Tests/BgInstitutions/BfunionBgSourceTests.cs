namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class BfunionBgSourceTests
    {
        [Theory]
        [InlineData("https://bfunion.bg/news/46252/0", "46252")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new BfunionBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithImage()
        {
            const string NewsUrl = "https://bfunion.bg/news/46256/0";
            var provider = new BfunionBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Честит рожден ден на Петко Петков", news.Title);
            Assert.Equal("46256", news.RemoteId);
            Assert.Equal(new DateTime(2019, 8, 3, 12, 59, 0), news.PostDate);
            Assert.Contains("Днес рожден ден празнува легендата на Берое Петко Петков.", news.Content);
            Assert.Contains("Българският футболен съюз честити празника на големия Петко Петков и му пожелава здраве, щастие и късмет!", news.Content);
            Assert.DoesNotContain("PetkoPetkov.png", news.Content);
            Assert.DoesNotContain("Август 2019", news.Content);
            Assert.Equal("https://bfunion.bg/uploads/2019-08-03/size1/PetkoPetkov.png", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithoutImage()
        {
            const string NewsUrl = "https://bfunion.bg/news/46255/0";
            var provider = new BfunionBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Решение СТК 02.08.2019", news.Title);
            Assert.Equal("46255", news.RemoteId);
            Assert.Equal(new DateTime(2019, 8, 2, 18, 11, 0).Date, news.PostDate.Date);
            Assert.Contains("БЪЛГАРСКИ ФУТБОЛЕН СЪЮЗ", news.Content);
            Assert.Contains("БФС си запазва правото на промени в програмата в зависимост от представянето на участниците в европейските турнири.", news.Content);
            Assert.DoesNotContain("1312-pm-timermans.jpg", news.Content);
            Assert.Null(news.ImageUrl);
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
