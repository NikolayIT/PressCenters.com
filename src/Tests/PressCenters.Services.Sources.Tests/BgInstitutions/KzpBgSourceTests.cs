namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class KzpBgSourceTests
    {
        [Theory]
        [InlineData("https://kzp.bg/novini/koe-kolko-struva-i-kak-da-sme-sigurni", "koe-kolko-struva-i-kak-da-sme-sigurni")]
        [InlineData("https://kzp.bg/novini/zabraneni-sa-neloyalni-praktiki-na-telekom", "zabraneni-sa-neloyalni-praktiki-na-telekom")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new KzpBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithImage()
        {
            const string NewsUrl = "https://kzp.bg/novini/bankomatat-glatna-kartata-kakvo-da-pravya";
            var provider = new KzpBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Банкоматът „глътна” картата – какво да правя?", news.Title);
            Assert.Equal("bankomatat-glatna-kartata-kakvo-da-pravya", news.RemoteId);
            Assert.Equal(new DateTime(2019, 8, 18), news.PostDate);
            Assert.Contains("Когато сме на почивка, променяме обичайния за ежедневието ни ритъм на живот.", news.Content);
            Assert.Contains("както при трансакции на суми от терминални устройства АТМ и ПОС, така и при загуба или кражба на картата.", news.Content);
            Assert.DoesNotContain("item_bankomat3.jpg", news.Content);
            Assert.DoesNotContain("18.08.2019", news.Content);
            Assert.Equal("https://kzp.bg/data/i/600x338x1/item_bankomat3.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithDefaultImage()
        {
            const string NewsUrl = "https://kzp.bg/novini/evropeyskata-komisiya-sas-zakonodatelno-predlozhenie-za-zashtitata-na-potrebitelite-v-oblastta-na-finansovite-uslugi";
            var provider = new KzpBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Европейската комисия със законодателно предложение за защитата на потребителите в областта на финансовите услуги", news.Title);
            Assert.Equal("evropeyskata-komisiya-sas-zakonodatelno-predlozhenie-za-zashtitata-na-potrebitelite-v-oblastta-na-finansovite-uslugi", news.RemoteId);
            Assert.Equal(new DateTime(2012, 7, 3), news.PostDate.Date);
            Assert.Contains("Финансовата криза се превърна в криза на доверието на потребителите", news.Content);
            Assert.Contains("за да се подсигури, че те винаги надхвърлят потенциалните ползи, произтичащи от нарушаването на разпоредбите.", news.Content);
            Assert.DoesNotContain("no-image.jpg", news.Content);
            Assert.Equal("https://kzp.bg/data/i/600x338x1/.no-image.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new KzpBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
