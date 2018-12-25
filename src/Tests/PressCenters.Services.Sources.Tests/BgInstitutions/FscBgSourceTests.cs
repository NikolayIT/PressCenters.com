namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class FscBgSourceTests
    {
        [Theory]
        [InlineData("http://www.fsc.bg/bg/novini/saobshtenie-za-klientite-na-tsitadela-kepital-menidzhmant-ood-7992.html", "7992")]
        [InlineData("http://www.fsc.bg/bg/novini/spisatsite-na-zastrahovatelite-i-zastrahovatelnite-posrednitsi-ot-darzhavi-chlenki-na-es-notifitsirali-kfn-sa-aktualizirani-7980.html", "7980")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new FscBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.fsc.bg/bg/novini/resheniya-ot-zasedanie-na-kfn-na-21-01-2016-g--7988.html";
            var provider = new FscBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Решения от заседание на КФН на 21.01.2016 г.", news.Title);
            Assert.Equal("7988", news.RemoteId);
            Assert.Equal(new DateTime(2016, 1, 22).Date, news.PostDate.Date);
            Assert.Contains("На заседанието си на 21.01.2016 г. КФН реши", news.Content);
            Assert.Contains("Република Италия и Република Португалия.", news.Content);
            Assert.True(!news.Content.Contains("_assets/img/banner.jpg"));
            Assert.True(!news.Content.Contains("Решения от заседание на КФН на 21.01.2016"));
            Assert.True(!news.Content.Contains("22/01/2016"));
            Assert.Equal("http://www.fsc.bg/_assets/img/banner.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new FscBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
