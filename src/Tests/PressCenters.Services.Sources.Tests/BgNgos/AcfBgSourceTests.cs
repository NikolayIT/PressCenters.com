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
        [InlineData("https://acf.bg/%d1%81%d0%b5%d0%b4%d0%b5%d0%bc-%d0%b3%d1%80%d0%b0%d0%b6%d0%b4%d0%b0%d0%bd%d1%81%d0%ba%d0%b8-%d0%be%d1%80%d0%b3%d0%b0%d0%bd%d0%b8%d0%b7%d0%b0%d1%86%d0%b8%d0%b8-%d0%bf%d1%80%d0%b5%d0%b4%d0%bb%d0%be/", "sedem-grazhdanski-organizatsii-predlo")]
        [InlineData("https://acf.bg/съдът-оправда-окончателно-сигнализи/", "sadat-opravda-okonchatelno-signalizi")]
        [InlineData("https://acf.bg/знак-за-бездействие-неизследваните-о/", "znak-za-bezdeystvie-neizsledvanite-o")]
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
            Assert.Equal("Въпроси към министър Младен Маринов във връзка с казуса „Осемте джуджета“", news.Title);
            Assert.Contains("По-долу публикуваме въпросите, които журналистът Николай Стайков от екипа", news.Content);
            Assert.Contains("бившия шеф на столичното следствие, сега адвокат, Петьо Петров. Моля за Вашия коментар.", news.Content);
            Assert.DoesNotContain("Абонирай се", news.Content);
            Assert.DoesNotContain("22 юли 2020", news.Content);
            Assert.Equal("https://acf.bg/wp-content/uploads/2020/07/New-Project-22.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2020, 7, 22), news.PostDate);
            Assert.Equal("vaprosi-kam-ministar-mladen-marinov-v", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithCyrillicUrlShouldWorkCorrectly()
        {
            const string NewsUrl = "https://acf.bg/%d1%81%d0%b5%d0%b4%d0%b5%d0%bc-%d0%b3%d1%80%d0%b0%d0%b6%d0%b4%d0%b0%d0%bd%d1%81%d0%ba%d0%b8-%d0%be%d1%80%d0%b3%d0%b0%d0%bd%d0%b8%d0%b7%d0%b0%d1%86%d0%b8%d0%b8-%d0%bf%d1%80%d0%b5%d0%b4%d0%bb%d0%be/";
            var provider = new AcfBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal("Седем граждански организации предложиха Андрей Янкулов и Таня Радуловска за членове на новия Висш съдебен съвет", news.Title);
            Assert.Contains("Седем граждански организации предложиха Андрей Янкулов и Таня", news.Content);
            Assert.Contains("за членове на Висшия съдебен съвет (ВСС) от квотата на Народното събрание.", news.Content);
            Assert.DoesNotContain("Абонирай се", news.Content);
            Assert.Equal("https://acf.bg/wp-content/uploads/2026/08/image-28.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 8, 26), news.PostDate);
            Assert.Equal("sedem-grazhdanski-organizatsii-predlo", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new AcfBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
