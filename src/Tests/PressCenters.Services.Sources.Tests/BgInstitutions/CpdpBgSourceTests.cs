namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CpdpBgSourceTests
    {
        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://cpdp.bg/завърши-първото-национално-студентс/";
            var provider = new CpdpBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal("Завърши първото национално студентско състезание по защитата на личните данни", news.Title);
            Assert.Contains("в Комисията за защита на личните данни (КЗЛД) се проведе финалният етап", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 9, 8, 19, 35), news.PostDate);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CpdpBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
