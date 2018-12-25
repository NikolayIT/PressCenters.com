namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class NsiBgNewsSourceTests
    {
        [Theory]
        [InlineData("http://www.nsi.bg/bg/content/13854/%D0%BC%D0%B8%D0%BD%D0%B8%D1%81%D1%82%D0%B5%D1%80%D1%81%D0%BA%D0%B8%D1%8F%D1%82-%D1%81%D1%8A%D0%B2%D0%B5%D1%82-%D0%BF%D1%80%D0%B8%D0%B5-%D0%BD%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%B0%D0%BB%D0%BD%D0%B0%D1%82%D0%B0-%D1%81%D1%82%D0%B0%D1%82%D0%B8%D1%81%D1%82%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B0-%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%B0-%D0%B7%D0%B0-2016-%D0%B3%D0%BE%D0%B4%D0%B8%D0%BD%D0%B0", "13854")]
        [InlineData("http://www.nsi.bg/bg/content/13854/somenewstitle/", "13854")]
        public void ExtractIdFromNewsUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new NsiBgNewsSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Theory]
        [InlineData("http://www.nsi.bg/bg/content/13840/somecategory/somenewstitle/", "13840")]
        [InlineData("http://www.nsi.bg/bg/content/13840/%D0%BF%D1%80%D0%B5%D1%81%D1%81%D1%8A%D0%BE%D0%B1%D1%89%D0%B5%D0%BD%D0%B8%D0%B5/%D0%B8%D0%BD%D1%84%D0%BB%D0%B0%D1%86%D0%B8%D1%8F-%D0%B8-%D0%B8%D0%BD%D0%B4%D0%B5%D0%BA%D1%81%D0%B8-%D0%BD%D0%B0-%D0%BF%D0%BE%D1%82%D1%80%D0%B5%D0%B1%D0%B8%D1%82%D0%B5%D0%BB%D1%81%D0%BA%D0%B8%D1%82%D0%B5-%D1%86%D0%B5%D0%BD%D0%B8-%D0%B7%D0%B0-%D0%B4%D0%B5%D0%BA%D0%B5%D0%BC%D0%B2%D1%80%D0%B8-2015-%D0%B3%D0%BE%D0%B4%D0%B8%D0%BD%D0%B0", "13840")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new NsiBgPressSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.nsi.bg/bg/content/13854/%D0%BC%D0%B8%D0%BD%D0%B8%D1%81%D1%82%D0%B5%D1%80%D1%81%D0%BA%D0%B8%D1%8F%D1%82-%D1%81%D1%8A%D0%B2%D0%B5%D1%82-%D0%BF%D1%80%D0%B8%D0%B5-%D0%BD%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%B0%D0%BB%D0%BD%D0%B0%D1%82%D0%B0-%D1%81%D1%82%D0%B0%D1%82%D0%B8%D1%81%D1%82%D0%B8%D1%87%D0%B5%D1%81%D0%BA%D0%B0-%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%B0-%D0%B7%D0%B0-2016-%D0%B3%D0%BE%D0%B4%D0%B8%D0%BD%D0%B0";
            var provider = new NsiBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министерският съвет прие Националната статистическа програма за 2016 година", news.Title);
            Assert.Contains("На заседание, проведено на 20 януари 2016", news.Content);
            Assert.Contains("След обнародването в „Държавен вестник” документите ще бъдат публикувани на интернет сайта на НСИ.", news.Content);
            Assert.DoesNotContain("___NSILogo_117.jpg", news.Content);
            Assert.Equal("http://www.nsi.bg/sites/default/files/styles/medium/public/files/events/images/___NSILogo_117.jpg?itok=Q9G_VKC0", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 21, 11, 45, 18), news.PostDate);
            Assert.Equal("13854", news.RemoteId);
        }

        [Fact]
        public void ParseRemotePressNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.nsi.bg/bg/content/13839/%D0%BF%D1%80%D0%B5%D1%81%D1%81%D1%8A%D0%BE%D0%B1%D1%89%D0%B5%D0%BD%D0%B8%D0%B5/%D0%B4%D0%B5%D0%B9%D0%BD%D0%BE%D1%81%D1%82-%D0%BD%D0%B0-%D0%BC%D0%B5%D1%81%D1%82%D0%B0%D1%82%D0%B0-%D0%B7%D0%B0-%D0%BD%D0%B0%D1%81%D1%82%D0%B0%D0%BD%D1%8F%D0%B2%D0%B0%D0%BD%D0%B5-%D0%BF%D1%80%D0%B5%D0%B7-%D0%BD%D0%BE%D0%B5%D0%BC%D0%B2%D1%80%D0%B8-2015-%D0%B3%D0%BE%D0%B4%D0%B8%D0%BD%D0%B0";
            var provider = new NsiBgPressSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Дейност на местата за настаняване през ноември 2015 година", news.Title);
            Assert.Contains("През ноември 2015 г. в страната са функционирали", news.Content);
            Assert.Contains("или с 0.8% повече в сравнение с ноември 2014 година.", news.Content);
            Assert.Equal("http://www.nsi.bg/sites/default/files/styles/medium/public/files/events/images/___NSILogo_117.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 14, 11, 0, 0), news.PostDate);
            Assert.Equal("13839", news.RemoteId);
        }

        [Fact]
        public void ParseRemotePressNewsShouldWorkCorrectlyWhenTextIsEmpty()
        {
            const string NewsUrl = "http://www.nsi.bg/bg/content/13808/%D0%BF%D1%80%D0%B5%D1%81%D1%81%D1%8A%D0%BE%D0%B1%D1%89%D0%B5%D0%BD%D0%B8%D0%B5/%D0%BA%D0%BB%D1%8E%D1%87%D0%BE%D0%B2%D0%B8-%D0%BF%D0%BE%D0%BA%D0%B0%D0%B7%D0%B0%D1%82%D0%B5%D0%BB%D0%B8-%D0%B7%D0%B0-%D0%B1%D1%8A%D0%BB%D0%B3%D0%B0%D1%80%D0%B8%D1%8F-%D0%BA%D1%8A%D0%BC-30122015-%D0%B3";
            var provider = new NsiBgPressSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Ключови показатели за България (към 30.12.2015 г.)", news.Title);
            Assert.Contains("http://www.nsi.bg/sites/default/files/files/pressreleases/KeyInd2015-12_2U0QR4H.pdf", news.Content);
            Assert.Equal("http://www.nsi.bg/sites/default/files/styles/medium/public/files/events/images/___NSILogo_117.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2015, 12, 30, 11, 0, 0), news.PostDate);
            Assert.Equal("13808", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new NsiBgNewsSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }

        [Fact]
        public void GetPressNewsShouldReturnResults()
        {
            var provider = new NsiBgPressSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
