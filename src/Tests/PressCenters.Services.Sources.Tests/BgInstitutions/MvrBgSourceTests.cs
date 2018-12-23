namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources;
    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class MvrBgSourceTests
    {
        [Fact]
        public void ExtractIdFromUrlShouldWorkCorrectly()
        {
            var provider = new MvrBgSource();
            var result = provider.ExtractIdFromUrl("http://press.mvr.bg/NEWS/news160121_06.htm");
            Assert.Equal("160121_06", result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://press.mvr.bg/NEWS/news170207_03.htm";
            var provider = new MvrBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Пътна полиция разработва карта на най-опасните пътища", news.Title);
            Assert.Equal("Целта е  анализ на причините и набелязване на мерки за намаляване на тежките пътни инциденти", news.ShortContent);
            Assert.Contains("най-опасните пътища в България, сочи статистика", news.Content);
            Assert.Contains("произшествия в района са намалели драстично.", news.Content);
            Assert.Equal("http://press.mvr.bg/NR/rdonlyres/6FCC2F41-FDB5-439A-BDFC-0E2ADB1FDBED/0/GDNPhome.gif", news.ImageUrl);
            Assert.Equal(new DateTime(2017, 2, 7, 0, 0, 0), news.PostDate);
            Assert.Equal("170207_03", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithRelatedDocumentsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://press.mvr.bg/Archive/News/News_2016/news160121_01.htm";
            var provider = new MvrBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal(
                "9969 нарушения на пътя са установени в страната от 11 до 17 януари",
                news.Title);
            Assert.Null(news.ShortContent);
            Assert.Contains("Според данните, предоставени от ГД", news.Content);
            Assert.Contains("безопасността на пътя", news.Content);
            Assert.Contains("http://press.mvr.bg/NR/rdonlyres/552F1CAD-22F3-4B23-B98F-62152420D6D0/0/CopyofDeynostPK1117012016.xls", news.Content);
            Assert.Equal("http://press.mvr.bg/NR/rdonlyres/60151086-F744-488C-91DC-A12130687CFC/0/POLICEISKA_KOLA_otpred.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 21, 0, 0, 0), news.PostDate);
            Assert.Equal("160121_01", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithOneOfTheFirstNews()
        {
            const string NewsUrl = "http://press.mvr.bg/Archive/News/News_2010/News_I_III_Q/news100106_02.htm";
            var provider = new MvrBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Полицаи задържаха почти 300 хил. кутии контрабандни цигари", news.Title);
            Assert.Equal("Контрабандният тютюн бил маскиран като шивашки материал, предотвратена е щета за бюджета от 1 млн. лв., задържан е шофьорът на товарния автомобил; при друга акция  в с. Катуница пловдивски полицаи и митничари разкриха към 500 бутилки с неистински бандерол \nПубликуваме снимки от операцията в Благоевград /1-3/; Катуница /4,5/", news.ShortContent);
            Assert.Contains("Български товарен автомобил, превозвал контрабандно цигари", news.Content);
            Assert.Contains("производство по чл. 244, ал. 1 от НК.", news.Content);
            Assert.Equal("http://press.mvr.bg/NR/rdonlyres/27D02D2F-393C-4662-8558-077602BA6F71/0/0106.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2010, 1, 6, 15, 38, 0), news.PostDate);
            Assert.Equal("100106_02", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MvrBgSource();
            var result = provider.GetLatestPublications(new LocalPublicationsInfo { LastLocalId = "160118_01" });
            Assert.True(result.News.Count() >= 9);
            Assert.True(
                result.LastNewsIdentifier.StartsWith(DateTime.Now.ToString("yyMMdd_"))
                || result.LastNewsIdentifier.StartsWith(DateTime.Now.AddDays(-1).ToString("yyMMdd_"))
                || result.LastNewsIdentifier.StartsWith(DateTime.Now.AddDays(-2).ToString("yyMMdd_"))
                || result.LastNewsIdentifier.StartsWith(DateTime.Now.AddDays(-3).ToString("yyMMdd_")));
        }

        [Fact]
        public void GetNewsShouldNotThrowAnExceptionWhenLastLocalIdIsBiggerOrEqualToTheCurrentLastNewsId()
        {
            var provider = new MvrBgSource();
            provider.GetLatestPublications(new LocalPublicationsInfo { LastLocalId = "999999_99" });
        }
    }
}
