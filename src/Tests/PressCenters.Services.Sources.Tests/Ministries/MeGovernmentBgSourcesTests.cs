namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MeGovernmentBgSourcesTests
    {
        [Theory]
        [InlineData("https://www.me.government.bg/bg/news/v-ministerstvoto-na-energetikata-se-provede-sreshta-vav-vrazka-s-podobryavane-na-elektrosnabdyavaneto-v-2686.html", "2686")]
        [InlineData("https://www.me.government.bg/bg/news/ministar-petkova-balgarskata-strana-shte-cherpi-opit-ot-avstriiskiya-centralnoevropeiski-gazov-hab-v-ra-2684.html?p=eyJ0eXBlIjoiaG90In0=", "2684")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var sources = new List<BaseSource> { new MeGovernmentBgNewsSource(), new MeGovernmentBgHotNewsSource(), };
            foreach (var source in sources)
            {
                var result = source.ExtractIdFromUrl(url);
                Assert.Equal(id, result);
            }
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.me.government.bg/bg/news/eksperti-ot-ministerstvoto-na-energetikata-proveriha-na-myasto-kachestvoto-na-elektropodavaneto-v-novi-2687.html";
            var provider = new MeGovernmentBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Експерти от Министерството на енергетиката провериха на място качеството на електроподаването в Нови Искър", news.Title);
            Assert.Contains("Със заповед на министъра на енергетиката Теменужка Петкова", news.Content);
            Assert.Contains("Проверката в района на екипа на Министерството на енергетиката продължава и утре, 11 януари 2019 г.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("news-2687-4345", news.Content);
            Assert.DoesNotContain("10.01.2019", news.Content);
            Assert.DoesNotContain("отпечатай тази страница", news.Content);
            Assert.DoesNotContain("обратно в списъка", news.Content);
            Assert.Equal(new DateTime(2019, 1, 10), news.PostDate);
            Assert.Equal("https://www.me.government.bg/files/news/image/news-2687-4345.jpg", news.ImageUrl);
            Assert.Equal("2687", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.me.government.bg/bg/news/ministerstvoto-na-energetikata-vze-merki-za-plashtane-na-vsichki-zabaveni-koncesionni-vaznagrajdeniya-2000.html?p=eyJ0eXBlIjoiaG90IiwicGFnZSI6OX0=";
            var provider = new MeGovernmentBgHotNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Обществено обсъждане на Наредба за условията и начина на изпълнение задълженията на производителите на електрическа енергия от топлоелектрически централи с комбинирано производство", news.Title);
            Assert.Contains("Министерството на енергетиката предлага за обществено обсъждане Наредба", news.Content);
            Assert.Contains(" в законоустановения срок.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("03.04.2015", news.Content);
            Assert.DoesNotContain("отпечатай тази страница", news.Content);
            Assert.DoesNotContain("обратно в списъка", news.Content);
            Assert.Equal(new DateTime(2015, 4, 3), news.PostDate);
            Assert.Equal("/images/sources/me.government.bg.png", news.ImageUrl);
            Assert.Equal("2000", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MeGovernmentBgNewsSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }

        [Fact]
        public void GetHotNewsShouldReturnResults()
        {
            var provider = new MeGovernmentBgHotNewsSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
