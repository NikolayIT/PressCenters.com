namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MpesGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("http://mpes.government.bg/Pages/Press/News/Default.aspx?evntid=Lr4t6iermgI%3d", "Lr4t6iermgI=")]
        [InlineData("http://mpes.government.bg/Pages/Press/News/Default.aspx?evntid=1234", "1234")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MpesGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://mpes.government.bg/Pages/Press/News/Default.aspx?evntid=Lr4t6iermgI%3d";
            var provider = new MpesGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Акценти от коментара на министъра на младежта и спорта Красен Кралев по основни теми в „Тази сутрин“ по БТВ", news.Title);
            Assert.Contains("Относно възможността България, Гърция, Румъния и Сърбия", news.Content);
            Assert.Contains("тази година. Аз съм оптимист\", посочи още Красен Кралев. ", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("<table", news.Content);
            Assert.DoesNotContain("Kralev.jpg", news.Content);
            Assert.DoesNotContain("09.01.2019", news.Content);
            Assert.DoesNotContain("Версия за печат", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.Equal(new DateTime(2019, 1, 9), news.PostDate);
            Assert.Equal("http://mpes.government.bg/Documents/PressCenter/News/2019/Krasen Kralev.jpg", news.ImageUrl);
            Assert.Equal("Lr4t6iermgI=", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://mpes.government.bg/Pages/Press/News/Default.aspx?evntid=600";
            var provider = new MpesGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("МФВС отделя 200 000 лева за спортуването на деца в риск и хора с увреждания", news.Title);
            Assert.Contains("За втора поредна година Министерството на физическото възпитание", news.Content);
            Assert.Contains("студентски спорт и спорт за хора с увреждания”.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("<table", news.Content);
            Assert.DoesNotContain("24.02.2012", news.Content);
            Assert.DoesNotContain("Версия за печат", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.Equal(new DateTime(2012, 2, 24), news.PostDate);
            Assert.Equal("/images/sources/mpes.government.bg.jpg", news.ImageUrl);
            Assert.Equal("600", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MpesGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(7, result.Count());
        }
    }
}
