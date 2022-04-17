namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class DamtnGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.damtn.government.bg/damtn-zavarshi-kampaniyata-za-svetoven-den-na-potrebitelya-2022", "damtn-zavarshi-kampaniyata-za-svetoven-den-na-potrebitelya-2022")]
        [InlineData("https://www.damtn.government.bg/na-vnimanieto-na-litsata-koito-podlezhat-na-proverka-ot-sluzhiteli-na-glavna-direktsiya-kontrol-na-kachestvoto-na-technite-goriva-gd-kktg-pri-darzhavna-agentsiya-za-metrologichen-i-tehnicheski-nadzor/", "na-vnimanieto-na-litsata-koito-podlezhat-na-proverka-ot-sluzhiteli-na-glavna-direktsiya-kontrol-na-kachestvoto-na-technite-goriva-gd-kktg-pri-darzhavna-agentsiya-za-metrologichen-i-tehnicheski-nadzor")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new DamtnGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.damtn.government.bg/svetoven-den-na-metrologiyata-20-maj-2022-g/";
            var provider = new DamtnGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Световен ден на метрологията – 20 май 2022 г.", news.Title);
            Assert.Equal("svetoven-den-na-metrologiyata-20-maj-2022-g", news.RemoteId);
            Assert.Equal(new DateTime(2022, 4, 8).Date, news.PostDate.Date);
            Assert.Contains("20 май е Международният ден на метрологията, който отбелязва годишнината от подписването", news.Content);
            Assert.Contains("Форма на презентация", news.Content);
            Assert.DoesNotContain("20-may-2-300x238", news.Content);
            Assert.DoesNotContain("Share this post", news.Content);
            Assert.Equal("https://www.damtn.government.bg/wp-content/uploads/2022/04/20-may-2-300x238.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithoutImage()
        {
            const string NewsUrl = "https://www.damtn.government.bg/damtn-zapochva-kampaniya-za-proverki-na-pazara-po-povod-den-na-potrebitelya-2022-g/";
            var provider = new DamtnGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("ДАМТН започва кампания за проверки на пазара по повод „Ден на потребителя 2022 г.“", news.Title);
            Assert.Equal("damtn-zapochva-kampaniya-za-proverki-na-pazara-po-povod-den-na-potrebitelya-2022-g", news.RemoteId);
            Assert.Equal(new DateTime(2022, 3, 14).Date, news.PostDate.Date);
            Assert.Contains("Инспектори на Главна дирекция „Надзор на пазара“ (ГД НП) при Държавна агенция", news.Content);
            Assert.Contains("продукти са били установени повече от едно несъответствие.", news.Content);
            Assert.DoesNotContain("Share this post", news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new DamtnGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
