namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class BasBgSourceTests
    {
        [Theory]
        [InlineData("https://www.bas.bg/?p=34369", "34369")]
        [InlineData("https://www.bas.bg?p=34381", "34381")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new BasBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.bas.bg/?p=34438";
            var provider = new BasBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Бродираният анимационен филм „Мармалад“ е селектиран за два международни фестивала", news.Title);
            Assert.Equal("34438", news.RemoteId);
            Assert.Equal(new DateTime(2021, 6, 2), news.PostDate);
            Assert.Contains("Бродираният анимационен филм „Мармалад“ с режисьор доц. д-р Радостина Нейкова от Института за изследване на изкуствата на БАН", news.Content);
            Assert.Contains("а композитор е Петко Манчев. Реализиран е с подкрепата на Национален филмов център.", news.Content);
            Assert.DoesNotContain("1-2-300x200.jpg", news.Content);
            Assert.DoesNotContain("2 юни 2021", news.Content);
            Assert.Equal("https://www.bas.bg/wp-content/uploads/2021/06/1-2-300x200.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly2017News()
        {
            const string NewsUrl = "https://www.bas.bg/?p=1227";
            var provider = new BasBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Българският отбор по математика завоюва първото място на Балканската олимпиада по математика", news.Title);
            Assert.Equal("1227", news.RemoteId);
            Assert.Equal(new DateTime(2017, 5, 9), news.PostDate.Date);
            Assert.Contains("Четири златни и два сребърни медала спечелиха българските участници в 34-тата Балканска олимпиада по математика, която се проведе от 2 до 7 май 2017 в Охрид, Македония.", news.Content);
            Assert.Contains("Забележителният успех на българския отбор е резултат от неуморните усилия на състезателите, техните ръководители и учители.", news.Content);
            Assert.DoesNotContain("bom2017", news.Content);
            Assert.DoesNotContain("9 май 2017", news.Content);
            Assert.Equal("https://www.bas.bg/wp-content/uploads/2017/06/bom2017-300x225.jpeg", news.ImageUrl);
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
