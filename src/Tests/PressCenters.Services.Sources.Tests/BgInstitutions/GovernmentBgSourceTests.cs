namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources;
    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class GovernmentBgSourceTests
    {
        [Theory]
        [InlineData("http://www.government.bg/bg/prestsentar/novini/premierat-boyko-borisov-v-belgrad-nyama-po-dobro-myasto-ot-es-i-nashite-sreshti-vinagi-sa-v-unison-s-evropeyskata-politika", "premierat-boyko-borisov-v-belgrad-nyama-po-dobro-myasto-ot-es-i-nashite-sreshti-vinagi-sa-v-unison-s-evropeyskata-politika")]
        [InlineData("http://www.government.bg/bg/prestsentar/novini/ministar-predsedatelyat-boyko-borisov-provede-telefonen-razgovor-s-darzhavniya-sekretar-na-sasht-mayk-pompeo", "ministar-predsedatelyat-boyko-borisov-provede-telefonen-razgovor-s-darzhavniya-sekretar-na-sasht-mayk-pompeo")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new GovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.government.bg/bg/prestsentar/novini/premierat-boyko-borisov-provede-dvustranna-sreshta-sas-zamestnik-predsedatelya-na-evropeyskata-komisiya-frans-timermans-v-bryuksel";
            var provider = new GovernmentBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Премиерът Бойко Борисов проведе двустранна среща със заместник-председателя на Европейската комисия Франс Тимерманс в Брюксел", news.Title);
            Assert.Equal("premierat-boyko-borisov-provede-dvustranna-sreshta-sas-zamestnik-predsedatelya-na-evropeyskata-komisiya-frans-timermans-v-bryuksel", news.RemoteId);
            Assert.Null(news.ShortContent);
            Assert.Equal(new DateTime(2018, 12, 13).Date, news.PostDate.Date);
            Assert.Contains("„Благодарих му за добрия доклад по Механизма за сътрудничество и проверка“", news.Content);
            Assert.Contains("подписахме, съгласихме се, тази тема трябва да приключи“, каза още премиерът Борисов.", news.Content);
            Assert.DoesNotContain("1312-pm-timermans.jpg", news.Content);
            Assert.Equal("http://www.government.bg/images/upload/13/768/1312-pm-timermans.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new GovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.News.Count() >= 12);
        }
    }
}
