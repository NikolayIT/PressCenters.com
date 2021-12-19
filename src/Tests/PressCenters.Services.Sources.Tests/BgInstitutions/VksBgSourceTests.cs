namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;
    using Xunit;

    public class VksBgSourceTests
    {
        [Theory]
        [InlineData("http://www.vks.bg/novini/ubiistvo_byala_cherkva_14_12.html", "ubiistvo_byala_cherkva_14_12")]
        [InlineData("http://www.vks.bg/novini/spisak-na-dela-ianuari.html", "spisak-na-dela-ianuari")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new VksBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.vks.bg/novini/ubiistvo_otvertkata_dupnitsa_14_12.html";
            var provider = new VksBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("ВКС върна на САС за ново разглеждане делото срещу Иван К., Владимир К. и Николай К. за убийството през 2014 г. в гр. Дупница на Николай Р. – Отвертката", news.Title);
            Assert.Contains("по наказателно дело № 236/2021 г. тричленен състав", news.Content);
            Assert.Contains("на решението в неговата цялост и връщане на делото за ново разглеждане.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.DoesNotContain("twitter", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal("ubiistvo_otvertkata_dupnitsa_14_12", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithAnOldNews()
        {
            const string NewsUrl = "http://www.vks.bg/novini/predsedateliat-komandirova-shestima-sadii.html";
            var provider = new VksBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Председателят на ВКС командирова шестима съдии от районни съдилища в страната да правораздават в Софийския районен съд", news.Title);
            Assert.Contains("Със свои заповеди от 15.01.2018 г. председателят на Върховния касационен съд (ВКС) комадирова шестима", news.Content);
            Assert.Contains("критерии за командироване на съдии в районните, окръжните, апелативните и специализираните съдилища.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.DoesNotContain("twitter", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal("predsedateliat-komandirova-shestima-sadii", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new VksBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
