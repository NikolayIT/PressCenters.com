namespace PressCenters.Services.Sources.Tests.BgNgos
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgNgos;

    using Xunit;

    public class AcfBgSourceTests
    {
        [Theory]
        [InlineData("https://acf.bg/bg/kmet-na-oblasten-grad-s-ekstravagantn/", "kmet-na-oblasten-grad-s-ekstravagantn")]
        [InlineData("https://acf.bg/bg/vaprosi-kam-ministar-mladen-marinov-v", "vaprosi-kam-ministar-mladen-marinov-v")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new AcfBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://acf.bg/bg/vaprosi-kam-ministar-mladen-marinov-v/";
            var provider = new AcfBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Въпроси към министър Младен Маринов във връзка с казуса \"Осемте джуджета\"", news.Title);
            Assert.Contains("По-долу публикуваме въпросите, които журналистът Николай Стайков от екипа", news.Content);
            Assert.Contains("бившия шеф на столичното следствие, сега адвокат, Петьо Петров. Моля за Вашия коментар.", news.Content);
            Assert.DoesNotContain("Сподели", news.Content);
            Assert.DoesNotContain("22.07.2020", news.Content);
            Assert.Equal("https://acf.bg/wp-content/uploads/2020/07/New-Project-22.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2020, 7, 22), news.PostDate);
            Assert.Equal("vaprosi-kam-ministar-mladen-marinov-v", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://acf.bg/bg/chast-3-na-osemte-dzhudzheta-edin-golyam-2/";
            var provider = new AcfBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Част 3 на „Осемте джуджета”: Един голям плик с евро за Еврото", news.Title);
            Assert.Contains("Историята на „Осемте джуджета” и намесата на бивши и настоящи магистрати в конфликта в групата фирми „Изамет” продължава", news.Content);
            Assert.Contains("към когото имаше анонимни обаждания на погребални теми и вандалски действия спрямо жилището му на 18 -19 юни 2020 г.", news.Content);
            Assert.DoesNotContain("Сподели", news.Content);
            Assert.DoesNotContain("30.07.2020", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2020, 7, 30), news.PostDate);
            Assert.Equal("chast-3-na-osemte-dzhudzheta-edin-golyam-2", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new AcfBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(3, result.Count());
        }
    }
}
