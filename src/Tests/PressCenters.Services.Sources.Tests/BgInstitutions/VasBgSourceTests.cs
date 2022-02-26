namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class VasBgSourceTests
    {
        [Theory]
        [InlineData("https://www.vas.bg/bg/a/do-obshchoto-sbranie-na-advokatite-ot-stranata", "do-obshchoto-sbranie-na-advokatite-ot-stranata")]
        [InlineData("https://www.vas.bg/bg/a/spisk-na-kandidatite-za-izpit-za-advokati-i-mladshi-advokati-dopusnati-do-vtorata-chast", "spisk-na-kandidatite-za-izpit-za-advokati-i-mladshi-advokati-dopusnati-do-vtorata-chast")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new VasBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.vas.bg/bg/a/edinodushno-ks-uvazhi-iskane-na-visshiya-advokatski-svet-otmeni-moratoriuma-za-pridobivane-po-davnost-na-drzhavni-i-obshchinski-imoti";
            var provider = new VasBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Единодушно КС уважи искане на Висшия адвокатски съвет - отмени мораториума за придобиване по давност на държавни и общински имоти", news.Title);
            Assert.Contains("Днес, 24.02.2022 г., Конституционният съд", news.Content);
            Assert.Contains("Решението е прието единодушно с 11 гласа", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("Актуално", news.Content);
            Assert.Equal("https://www.vas.bg/p/k/s/ks-nov-11543-825x0.png", news.ImageUrl);
            Assert.Equal("edinodushno-ks-uvazhi-iskane-na-visshiya-advokatski-svet-otmeni-moratoriuma-za-pridobivane-po-davnost-na-drzhavni-i-obshchinski-imoti", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.vas.bg/bg/a/obrbshchenie-po-povod-zastrakhovka-profesionalna-otgovornost-na-advokatite";
            var provider = new VasBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("ОБРЪЩЕНИЕ по повод застраховка \"Професионална отговорност\" на адвокатите", news.Title);
            Assert.Contains("Във връзка с подготвяния договор за застраховка", news.Content);
            Assert.Contains("Призоваваме всички, които имат интерес от присъединяване към договора за застраховка да предприемат необходимите действия", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("Актуално", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal("obrbshchenie-po-povod-zastrakhovka-profesionalna-otgovornost-na-advokatite", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new VasBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
