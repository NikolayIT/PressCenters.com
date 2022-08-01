namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class DvParliamentBgSourceTests
    {
        [Fact]
        public void GetLatestPublicationsShouldReturnCurrentIssue()
        {
            var provider = new DvParliamentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Single(result);
            var news = result.First();
            Assert.NotNull(news.Title);
            Assert.NotNull(news.Content);
            Assert.Contains("Брой: ", news.Title);
            Assert.Contains(" от дата ", news.Title);
            Assert.Contains("Преглед на материала", news.Content);
        }
    }
}
