namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class BasBgSourceTests
    {
        [Theory]
        [InlineData("http://www.bas.bg/2017/10/16/%d0%bf%d0%be%d0%b7%d0%b8%d1%86%d0%b8%d1%8f-%d0%bd%d0%b0-%d1%80%d1%8a%d0%ba%d0%be%d0%b2%d0%be%d0%b4%d1%81%d1%82%d0%b2%d0%be%d1%82%d0%be-%d0%bd%d0%b0-%d0%b1%d0%b0%d0%bd/", "2017/10/16/позиция-на-ръководството-на-бан")]
        [InlineData("http://www.bas.bg/2019/07/24/специализиран-научноизследователск", "2019/07/24/специализиран-научноизследователск")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new BasBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.bas.bg/2019/08/19/%d1%87%d0%b5%d1%82%d0%b8%d1%80%d0%b8-%d0%bc%d0%b5%d0%b4%d0%b0%d0%bb%d0%b0-%d0%b7%d0%b0-%d0%b1%d1%8a%d0%bb%d0%b3%d0%b0%d1%80%d0%b8%d1%8f-%d0%b2-%d0%bc%d0%b5%d0%b6%d0%b4%d1%83%d0%bd%d0%b0%d1%80%d0%be/";
            var provider = new BasBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Четири медала за България в международната олимпиада по информатика в Баку", news.Title);
            Assert.Equal("2019/08/19/четири-медала-за-българия-в-междунаро", news.RemoteId);
            Assert.Equal(new DateTime(2019, 8, 19), news.PostDate);
            Assert.Contains("Четири медала завоюваха българските участници в международната олимпиада по информатика, която се проведе от 3 до 11 август в гр. Баку, Азербайджан.", news.Content);
            Assert.Contains("Страната ни запазва 5-то място в света по медали в класирането за всичките олимпиади.", news.Content);
            Assert.DoesNotContain("IMG_0214-300x225.jpg", news.Content);
            Assert.DoesNotContain("19 август 2019", news.Content);
            Assert.Equal("http://www.bas.bg/wp-content/uploads/2019/08/IMG_0214-300x225.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly2017News()
        {
            const string NewsUrl = "http://www.bas.bg/2017/05/09/%d0%b1%d1%8a%d0%bb%d0%b3%d0%b0%d1%80%d1%81%d0%ba%d0%b8%d1%8f%d1%82-%d0%be%d1%82%d0%b1%d0%be%d1%80-%d0%bf%d0%be-%d0%bc%d0%b0%d1%82%d0%b5%d0%bc%d0%b0%d1%82%d0%b8%d0%ba%d0%b0-%d0%b7%d0%b0%d0%b2%d0%be/";
            var provider = new BasBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Българският отбор по математика завоюва първото място на Балканската олимпиада по математика", news.Title);
            Assert.Equal("2017/05/09/българският-отбор-по-математика-заво", news.RemoteId);
            Assert.Equal(new DateTime(2017, 5, 9), news.PostDate.Date);
            Assert.Contains("Четири златни и два сребърни медала спечелиха българските участници в 34-тата Балканска олимпиада по математика, която се проведе от 2 до 7 май 2017 в Охрид, Македония.", news.Content);
            Assert.Contains("Забележителният успех на българския отбор е резултат от неуморните усилия на състезателите, техните ръководители и учители.", news.Content);
            Assert.DoesNotContain("bom2017", news.Content);
            Assert.DoesNotContain("9 май 2017", news.Content);
            Assert.Equal("http://www.bas.bg/wp-content/uploads/2017/06/bom2017-300x225.jpeg", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new BasBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
