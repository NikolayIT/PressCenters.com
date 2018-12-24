namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

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
            const string NewsUrl = "https://www.prb.bg/bg/news/aktualno/apelativna-prokuratura-burgas-protestira-reshe-174/";
            var provider = new PrbBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Апелативна прокуратура – Бургас протестира решение на съда, с което е изменена присъдата и намалено наказанието на Кирил Х., причинил смъртта на мъж, работил в неговия ресторант", news.Title);
            Assert.Equal("aktualno/apelativna-prokuratura-burgas-protestira-reshe-174", news.RemoteId);
            Assert.Null(news.ShortContent);
            Assert.Equal(new DateTime(2018, 12, 21), news.PostDate.Date);
            Assert.Contains("Със свое решение от 30.11.2018 г.", news.Content);
            Assert.Contains("Тази част от съдебното решение е протестирана", news.Content);
            Assert.Contains("при „строг“ режим в затвор.", news.Content);
            Assert.DoesNotContain("Апелативна прокуратура – Бургас протестира решение на съда, с което е изменена присъдата и намалено наказанието на Кирил Х., причинил смъртта на мъж, работил в неговия ресторант", news.Content);
            Assert.DoesNotContain("12.18", news.Content);
            Assert.Equal("https://www.prb.bg/media/uploaded_images/thumb_770x0_Съдебна палата Бургас_84.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new PrbBgSource();
            var result = provider.GetLatestPublications();

            Assert.True(result.Count() >= 10);
        }
    }
}
