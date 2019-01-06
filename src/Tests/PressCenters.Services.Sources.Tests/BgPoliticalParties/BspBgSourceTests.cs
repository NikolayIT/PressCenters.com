namespace PressCenters.Services.Sources.Tests.BgPoliticalParties
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgPoliticalParties;

    using Xunit;

    public class BspBgSourceTests
    {
        [Theory(Skip = "Source excluded")]
        [InlineData("https://bsp.bg/news/view/11653-korneliya_ninova_za_seta__vredno_e__govorim_ot_imeto_na_hilyadi_bylgari.html", "11653")]
        [InlineData("https://bsp.bg/news/view/11649-korneliya_ninova__6_partii_vnesohme_6000_podpisa_za_otlichen_6_na_izborite.html", "11649")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new BspBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact(Skip = "Source excluded")]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://bsp.bg/news/view/11649-korneliya_ninova__6_partii_vnesohme_6000_podpisa_za_otlichen_6_na_izborite.html";
            var provider = new BspBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Корнелия Нинова: 6 партии внесохме 6000 подписа за отличен 6 на изборите", news.Title);
            Assert.Equal("11649", news.RemoteId);
            Assert.Equal(new DateTime(2017, 2, 8), news.PostDate);
            Assert.Contains("6 партии в коалиция", news.Content);
            Assert.Contains("Корнелия Нинова подчерта, че", news.Content);
            Assert.Contains("политики; икономика, здравеопазване, социална политика и сигурност.", news.Content);
            Assert.Contains("Въпросът с кого ще управляваме ще го решаваме с питане до цялата партия", news.Content);
            Assert.DoesNotContain("Корнелия Нинова: 6 партии внесохме 6000 подписа за отличен 6 на изборите", news.Content);
            Assert.DoesNotContain("Фев 08, 2017", news.Content);
            Assert.DoesNotContain("https://apis.google.com/js/platform.js", news.Content);
            Assert.Equal("https://bsp.bg/files/news/small/19d23c6b4a492a0f6d511624a45f6286.jpg", news.ImageUrl);
        }

        [Fact(Skip = "Source excluded")]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new BspBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(9, result.Count());
        }
    }
}
