namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class EgovGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://midt.gov.bg/bg/news/1590", "1590")]
        [InlineData("https://midt.gov.bg/bg/news/196/", "196")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new EgovGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://midt.gov.bg/bg/news/1590";
            var provider = new EgovGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Василев: Иновациите и стратегическите партньорства са ключови за сигурността на Европа", news.Title);
            Assert.Contains("Министерството на иновациите и дигиталната трансформация е институционален партньор", news.Content);
            Assert.Contains("Регионалната среща на върха по въпросите на отбраната и сигурността 2026", news.Content);
            Assert.DoesNotContain("20261021_131448.jpg", news.Content);
            Assert.DoesNotContain("Зам.-министър Йосифова", news.Content);
            Assert.Equal("https://midt.gov.bg/upload/4015/20261021_131448.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 10, 21), news.PostDate);
            Assert.Equal("1590", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithAbbreviatedMarchShouldWorkCorrectly()
        {
            const string NewsUrl = "https://midt.gov.bg/bg/news/196";
            var provider = new EgovGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal("Министър Младенова и посланикът на Турция обсъдиха задълбочаване на сътрудничеството в сферата на инвестициите и иновациите", news.Title);
            Assert.Contains("Инвестициите на турски компании у нас достигат близо 3 млрд. долара", news.Content);
            Assert.Equal("https://midt.gov.bg/upload/210/1-4.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 3, 31), news.PostDate);
            Assert.Equal("196", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new EgovGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
