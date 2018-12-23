namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources;
    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class PrbBgSourceTests
    {
        [Theory]
        [InlineData("http://www.prb.bg/bg/news/aktualno/okrzhna-prokuratura-plovdiv-vnese-dva-obviniteln-3/", "aktualno/okrzhna-prokuratura-plovdiv-vnese-dva-obviniteln-3")]
        [InlineData("http://www.prb.bg/bg/news/aktualno/sled-sporazumenie-s-rajonnata-prokuratura-vv-var-3", "aktualno/sled-sporazumenie-s-rajonnata-prokuratura-vv-var-3")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new PrbBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.prb.bg/bg/news/aktualno/okrzhna-prokuratura-plovdiv-vnese-dva-obviniteln-3/";
            var provider = new PrbBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Окръжна прокуратура - Пловдив внесе два обвинителни акта за грабежи, извършени от рецидивисти", news.Title);
            Assert.Equal("aktualno/okrzhna-prokuratura-plovdiv-vnese-dva-obviniteln-3", news.RemoteId);
            Assert.Null(news.ShortContent);
            Assert.Equal(new DateTime(2017, 2, 6, 16, 39, 40), news.PostDate);
            Assert.Contains("Окръжна прокуратура - Пловдив внесе в съда обвинителен акт", news.Content);
            Assert.Contains("направил опит и отнел чужди движими вещи", news.Content);
            Assert.Contains("мярка за неотклонение „задържане под стража“.", news.Content);
            Assert.DoesNotContain("<h1>Окръжна прокуратура - Пловдив внесе два обвинителни акта за грабежи, извършени от рецидивисти", news.Content);
            Assert.DoesNotContain("6 Фев. 2017", news.Content);
            Assert.Equal("http://www.prb.bg/media/uploaded_images/Пловдив_2_iLwoIwI.jpg.720x420_q85_box-21%2C0%2C619%2C400_crop_detail.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new PrbBgSource();
            var result = provider.GetLatestPublications(new LocalPublicationsInfo { LastLocalId = string.Empty });

            Assert.True(result.News.Count() >= 10);
        }
    }
}
