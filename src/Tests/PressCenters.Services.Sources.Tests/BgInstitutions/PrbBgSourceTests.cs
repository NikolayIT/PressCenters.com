namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class PrbBgSourceTests
    {
        [Theory]
        [InlineData("http://prb.bg/bg/news/aktualno/okrzhna-prokuratura-plovdiv-vnese-dva-obviniteln-3/", "aktualno/okrzhna-prokuratura-plovdiv-vnese-dva-obviniteln-3")]
        [InlineData("http://prb.bg/bg/news/aktualno/sled-sporazumenie-s-rajonnata-prokuratura-vv-var-3", "aktualno/sled-sporazumenie-s-rajonnata-prokuratura-vv-var-3")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new PrbBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://prb.bg/bg/news/aktualno/31661-apelativna-prokuratura-burgas-protestira-reshe-174/";
            var provider = new PrbBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Апелативна прокуратура – Бургас протестира решение на съда, с което е изменена присъдата и намалено наказанието на Кирил Х., причинил смъртта на мъж, работил в неговия ресторант", news.Title);
            Assert.Equal("aktualno/31661-apelativna-prokuratura-burgas-protestira-reshe-174", news.RemoteId);
            Assert.Equal(new DateTime(2018, 12, 21), news.PostDate.Date);
            Assert.Contains("Със свое решение от 30.11.2018 г.", news.Content);
            Assert.Contains("Тази част от съдебното решение е протестирана", news.Content);
            Assert.Contains("при „строг“ режим в затвор.", news.Content);
            Assert.DoesNotContain("Апелативна прокуратура – Бургас протестира решение на съда, с което е изменена присъдата и намалено наказанието на Кирил Х., причинил смъртта на мъж, работил в неговия ресторант", news.Content);
            Assert.DoesNotContain("12.18", news.Content);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://prb.bg/bg/news/aktualno/819-vrkhovnata-administrativna-prokuratura-vap-ob-1355";
            var provider = new PrbBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Върховната административна прокуратура (ВАП) обобщи резултатите ...", news.Title);
            Assert.Equal("aktualno/819-vrkhovnata-administrativna-prokuratura-vap-ob-1355", news.RemoteId);
            Assert.Equal(new DateTime(2008, 12, 2), news.PostDate.Date);
            Assert.Contains("Върховната административна прокуратура (ВАП) обобщи", news.Content);
            Assert.Contains("По указание на ВАП окръжните прокуратури в страната", news.Content);
            Assert.Contains("Видно от обобщените от ВАП резултати", news.Content);
            Assert.Contains("по упражняване на стопанска дейност от търговските субекти.", news.Content);
            Assert.DoesNotContain("12.08", news.Content);
            Assert.Equal("https://prb.bg/assets/images/temp-img.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsFrom2020ShouldWorkCorrectly()
        {
            const string NewsUrl = "https://prb.bg/bg/news/aktualno/44611-spetsializirana-prokuratura-obrazuva-dosadebno-proizvodstvo-za-prestaplenie-po-g";
            var provider = new PrbBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Специализирана прокуратура образува досъдебно производство за престъпление по глава Първа от НК", news.Title);
            Assert.Equal("aktualno/44611-spetsializirana-prokuratura-obrazuva-dosadebno-proizvodstvo-za-prestaplenie-po-g", news.RemoteId);
            Assert.Equal(new DateTime(2020, 7, 14), news.PostDate.Date);
            Assert.Contains("Васил Божков и други лица", news.Content);
            Assert.Contains("спазване на закрепеното в чл. 41 от Конституцията на Република България", news.Content);
            Assert.Contains("може да възстанови доверието на обществото към правовия ред и да докаже, че никои не е над закона.", news.Content);
            Assert.DoesNotContain("Актуално", news.Content);
            Assert.Equal("https://prb.bg/upload/41260/АСП.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsFrom2021ShouldWorkCorrectly()
        {
            const string NewsUrl = "https://prb.bg/bg/news/aktualno/54173-okrazhna-prokuratura-razgrad-rakovodi-dosadebno-proizvodstvo-vav-vrazka-s-tezhak";
            var provider = new PrbBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Окръжна прокуратура-Разград ръководи досъдебно производство във връзка с тежък пътен инцидент със загинал пешеходец", news.Title);
            Assert.Equal("aktualno/54173-okrazhna-prokuratura-razgrad-rakovodi-dosadebno-proizvodstvo-vav-vrazka-s-tezhak", news.RemoteId);
            Assert.Equal(new DateTime(2021, 11, 2), news.PostDate.Date);
            Assert.Contains("Окръжна прокуратура-Разград ръководи досъдебно производство", news.Content);
            Assert.Contains("управлявал МПС след употреба на алкохол или наркотични вещества.", news.Content);
            Assert.DoesNotContain("Актуално", news.Content);
            Assert.Equal("https://prb.bg/upload/55510/Района+прокуратура+Разград.JPG", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new PrbBgSource();
            var result = provider.GetLatestPublications();

            Assert.Equal(10, result.Count());
        }
    }
}
