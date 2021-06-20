namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class BnbBgSourceTests
    {
        [Theory]
        [InlineData("https://bnb.bg/PressOffice/POPressReleases/POPRDate/PR_20210611_BG", "PR_20210611_BG")]
        [InlineData("https://bnb.bg/PressOffice/POPressReleases/POPRDate/PR_20121218_BG/", "PR_20121218_BG")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new BnbBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://bnb.bg/PressOffice/POPressReleases/POPRDate/PR_20210615_BG";
            var provider = new BnbBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("БНБ публикува статистически данни за май 2021 г. за структурата на банкнотите и разменните монети в обращение", news.Title);
            Assert.Equal("PR_20210615_BG", news.RemoteId);
            Assert.Equal(new DateTime(2021, 6, 15), news.PostDate);
            Assert.Contains("Данните са систематизирани в динамични редове", news.Content);
            Assert.Contains("Календарът за тяхното разпространение е достъпен", news.Content);
            Assert.DoesNotContain("ПРЕССЪОБЩЕНИЕ", news.Content);
            Assert.DoesNotContain("15 юни 2021 г.", news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithAdditionalInfo()
        {
            const string NewsUrl = "https://bnb.bg/PressOffice/POPressReleases/POPRDate/PR_20210618_10LV_BG";
            var provider = new BnbBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("БНБ пуска в обращение сребърна възпоменателна монета „100 години Национална музикална академия“", news.Title);
            Assert.Equal("PR_20210618_10LV_BG", news.RemoteId);
            Assert.Equal(new DateTime(2021, 6, 18), news.PostDate.Date);
            Assert.Contains("От 21 юни 2021 г. Българската народна банка пуска в обращение", news.Content);
            Assert.Contains("Списък на офиси и клонове на банки, в които ще се продава новата възпоменателна монета", news.Content);
            Assert.DoesNotContain("ПРЕССЪОБЩЕНИЕ", news.Content);
            Assert.DoesNotContain("18 юни 2021 г.", news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new BnbBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
