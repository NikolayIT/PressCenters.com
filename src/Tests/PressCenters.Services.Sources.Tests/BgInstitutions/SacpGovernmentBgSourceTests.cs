namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class SacpGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://sacp.government.bg/%D0%BD%D0%BE%D0%B2%D0%B8%D0%BD%D0%B8/%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%BD%D0%B0-%D0%B3%D1%80%D1%83%D0%BF%D0%B0-%D1%80%D0%B0%D0%B7%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B2%D0%B0", "работна-група-разработва")]
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
            const string NewsUrl = "https://sacp.government.bg/%D0%BD%D0%BE%D0%B2%D0%B8%D0%BD%D0%B8/%D0%B4%D1%8A%D1%80%D0%B6%D0%B0%D0%B2%D0%BD%D0%B0%D1%82%D0%B0-%D0%B0%D0%B3%D0%B5%D0%BD%D1%86%D0%B8%D1%8F-%D0%B7%D0%B0-%D0%B7%D0%B0%D0%BA%D1%80%D0%B8%D0%BB%D0%B0-28";
            var provider = new SacpGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Държавната агенция за закрила на детето организира майсторски клас „Иновации и отговорности в дигиталния свят с децата“", news.Title);
            Assert.Equal("държавната-агенция-за-закрила-28", news.RemoteId);
            Assert.Equal(new DateTime(2022, 6, 14).Date, news.PostDate.Date);
            Assert.Contains("Правата на децата в дигитална среда и отговорности на професионалистите", news.Content);
            Assert.Contains("детски градини от различни градове в страната – София, Кюстендил и Русе.", news.Content);
            Assert.DoesNotContain("14.06.2022", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.Equal("https://sacp.government.bg/sites/default/files/styles/1920x832/public/news/novini-zakrila-na-deteto-4078.jpg?itok=Xurve3Or", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithoutImage()
        {
            const string NewsUrl = "https://sacp.government.bg/%D0%BD%D0%BE%D0%B2%D0%B8%D0%BD%D0%B8/%D0%B4%D0%B0%D0%B7%D0%B4-%D0%BF%D0%BE%D0%B4%D1%81%D0%B8%D0%BB%D0%B2%D0%B0-%D0%B5%D0%BA%D0%B8%D0%BF%D0%B0-%D1%81%D0%B8-%D0%BE%D1%82";
            var provider = new SacpGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("ДАЗД подсилва екипа си от специалисти по кризисна интервенция", news.Title);
            Assert.Equal("дазд-подсилва-екипа-си-от", news.RemoteId);
            Assert.Equal(new DateTime(2020, 4, 9).Date, news.PostDate.Date);
            Assert.Contains("По време на усложнената епидемична обстановка Държавната агенция за закрила на детето продължава", news.Content);
            Assert.Contains("Желаещите да се включат могат да се обърнат към ДАЗД в срок до края на месец април 2020 г.", news.Content);
            Assert.DoesNotContain("09.04.2020", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.Equal("https://sacp.government.bg/themes/custom/dazd/dist/images/node_default_image.jpg", news.ImageUrl);
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
