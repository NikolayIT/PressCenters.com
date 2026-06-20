namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class SacpGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://sacp.government.bg/news/some-news-article-657-2", "some-news-article-657-2")]
        [InlineData("https://sacp.government.bg/новини/председателят-на-дазд-изпрати-1", "председателят-на-дазд-изпрати-1")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new SacpGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://sacp.government.bg/news/d-r-teodora-ivanova:-da-bydem-otgovorni-kym-decata-ni-chestit-1-yuni-269-2";
            var provider = new SacpGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("Д-р Теодора Иванова", news.Title);
            Assert.Equal("d-r-teodora-ivanova:-da-bydem-otgovorni-kym-decata-ni-chestit-1-yuni-269-2", news.RemoteId);
            Assert.Equal(new DateTime(2026, 6, 1).Date, news.PostDate.Date);
            Assert.Contains("Днес отбелязваме Международния ден на детето", news.Content);
            Assert.Contains("няма по-голяма отговорност от това да я пазим.", news.Content);
            Assert.StartsWith("https://sacp.government.bg/image/cache/catalog/", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://sacp.government.bg/news/iniciativa-sreshtu-opasnite-selfita-i-poseshtenie-vyv-voennomedicinskiq-simulacionen-trenirovychen-centyr-na-vma-897-2";
            var provider = new SacpGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.StartsWith("Инициатива срещу опасните селфита", news.Title);
            Assert.Equal("iniciativa-sreshtu-opasnite-selfita-i-poseshtenie-vyv-voennomedicinskiq-simulacionen-trenirovychen-centyr-na-vma-897-2", news.RemoteId);
            Assert.Equal(new DateTime(2026, 5, 28).Date, news.PostDate.Date);
            Assert.Contains("Достъпът на децата до изкуство, застъпничеството сред младежите", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://sacp.government.bg/image/cache/catalog/", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new SacpGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
