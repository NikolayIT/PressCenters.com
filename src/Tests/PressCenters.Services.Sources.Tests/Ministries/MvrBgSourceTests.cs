namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MvrBgSourceTests
    {
        private readonly List<BaseSource> sources = new List<BaseSource>
                                                    {
                                                        new MvrBgAktualnoSource(),
                                                        new MvrBgNoviniSource(),
                                                        new MvrBgInformacionenBiuletinSource(),
                                                        new MvrBgPutnaObstanovkaSource(),
                                                    };

        [Theory]
        [InlineData("https://www.mvr.bg/press/актуална-информация/актуална-информация/новини/преглед/новини/създадена-е-организация-за-спокойното-протичане-на-идните-празнични-и-почивни-дни", "новини/създадена-е-организация-за-спокойното-протичане-на-идните-празнични-и-почивни-дни")]
        [InlineData("https://www.mvr.bg/press/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/%D0%BF%D1%80%D0%B5%D0%B3%D0%BB%D0%B5%D0%B4/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/%D0%B2%D0%BD%D0%B8%D0%BC%D0%B0%D0%BD%D0%B8%D0%B5-%D0%BE%D0%BF%D0%B0%D1%81%D0%BD%D0%BE%D1%81%D1%82-%D0%BE%D1%82-%D0%BF%D0%BE%D0%B6%D0%B0%D1%80%D0%B8-%D0%B2-%D0%B1%D0%B8%D1%82%D0%B0!", "актуално/внимание-опасност-от-пожари-в-бита!")]
        [InlineData("https://www.mvr.bg/press/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%BF%D1%8A%D1%82%D0%BD%D0%B0-%D0%BE%D0%B1%D1%81%D1%82%D0%B0%D0%BD%D0%BE%D0%B2%D0%BA%D0%B0/%D0%BF%D1%80%D0%B5%D0%B3%D0%BB%D0%B5%D0%B4/%D0%BF%D1%8A%D1%82%D0%BD%D0%B0-%D0%BE%D0%B1%D1%81%D1%82%D0%B0%D0%BD%D0%BE%D0%B2%D0%BA%D0%B0/100101_01", "пътна-обстановка/100101_01")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            foreach (var source in this.sources)
            {
                var result = source.ExtractIdFromUrl(url);
                Assert.Equal(id, result);
            }
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mvr.bg/press/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/%D0%BF%D1%80%D0%B5%D0%B3%D0%BB%D0%B5%D0%B4/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/%D1%80%D1%8F%D0%B7%D0%BA%D0%B0%D1%82%D0%B0-%D0%BF%D1%80%D0%BE%D0%BC%D1%8F%D0%BD%D0%B0-%D0%BD%D0%B0-%D0%B2%D1%80%D0%B5%D0%BC%D0%B5%D1%82%D0%BE-%D0%BA%D1%80%D0%B8%D0%B5-%D0%BE%D0%BF%D0%B0%D1%81%D0%BD%D0%BE%D1%81%D1%82%D0%B8-%D0%BD%D0%B0-%D0%BF%D1%8A%D1%82%D1%8F";
            var provider = new MvrBgAktualnoSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Рязката промяна на времето крие опасности на пътя", news.Title);
            Assert.Contains("Пътна полиция съветва: Поемайте на път добре подготвени", news.Content);
            Assert.Contains("Времето осезателно се промени днес, за утре се очакват значителни валежи от дъжд в южните райони, и от сняг", news.Content);
            Assert.Contains("При попадане в рискова ситуация сигнализирайте първо на телефона за спешни случаи 112, след това потърсете за съдействие близки и приятели.", news.Content);
            Assert.DoesNotContain("https://www.mvr.bg/GetImage.ashx?id=d4f8176a-def9-42d8-8f24-ce0d3c503554&height=460&width=1260", news.Content);
            Assert.DoesNotContain("<script>", news.Content);
            Assert.DoesNotContain("glyphicon-edit", news.Content);
            Assert.DoesNotContain("<legend>Изображения</legend>", news.Content);
            Assert.Equal("https://www.mvr.bg/GetImage.ashx?id=d4f8176a-def9-42d8-8f24-ce0d3c503554&height=460&width=1260", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 11, 27, 17, 43, 0), news.PostDate);
            Assert.Equal("актуално/рязката-промяна-на-времето-крие-опасности-на-пътя", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mvr.bg/press/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/%D0%BF%D1%80%D0%B5%D0%B3%D0%BB%D0%B5%D0%B4/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/72-%D0%B3%D0%BE%D0%B4%D0%B8%D1%88%D0%B5%D0%BD-%D0%B5-%D0%BE%D1%82%D0%B2%D0%B5%D0%B4%D0%B5%D0%BD-%D0%B2-%D1%81%D1%82%D0%BE%D0%BB%D0%B8%D1%87%D0%BD%D0%BE%D1%82%D0%BE-%D0%BF%D1%8A%D1%80%D0%B2%D0%BE-%D1%80%D0%B0%D0%B9%D0%BE%D0%BD%D0%BD%D0%BE-%D1%83%D0%BF%D1%80%D0%B0%D0%B2%D0%BB%D0%B5%D0%BD%D0%B8%D0%B5";
            var provider = new MvrBgAktualnoSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("72-годишен е отведен в столичното Първо районно управление", news.Title);
            Assert.Contains("В късния следобед днес 72-годишен мъж дошъл на входа на Президентството и поискал среща.", news.Content);
            Assert.Contains("Образувано е досъдебно производство, изясняват се всички обстоятелства по случая.", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 3), news.PostDate);
            Assert.Equal("актуално/72-годишен-е-отведен-в-столичното-първо-районно-управление", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsInformacionenBiuletinShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mvr.bg/press/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%B5%D0%BD-%D0%B1%D1%8E%D0%BB%D0%B5%D1%82%D0%B8%D0%BD/%D0%BF%D1%80%D0%B5%D0%B3%D0%BB%D0%B5%D0%B4/%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%B5%D0%BD-%D0%B1%D1%8E%D0%BB%D0%B5%D1%82%D0%B8%D0%BD/bl100102_01";
            var provider = new MvrBgInformacionenBiuletinSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Информационен бюлетин", news.Title);
            Assert.Contains("Двама пияни шофьори са заловени", news.Content);
            Assert.Contains("ЦИТИРАТЕ КОРЕКТНО ИНФОРМАЦИИТЕ НА ПРЕСЦЕНТЪРА!", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2010, 1, 2), news.PostDate);
            Assert.Equal("информационен-бюлетин/bl100102_01", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsPutnaObstanovkaShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mvr.bg/press/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%BF%D1%8A%D1%82%D0%BD%D0%B0-%D0%BE%D0%B1%D1%81%D1%82%D0%B0%D0%BD%D0%BE%D0%B2%D0%BA%D0%B0/%D0%BF%D1%80%D0%B5%D0%B3%D0%BB%D0%B5%D0%B4/%D0%BF%D1%8A%D1%82%D0%BD%D0%B0-%D0%BE%D0%B1%D1%81%D1%82%D0%B0%D0%BD%D0%BE%D0%B2%D0%BA%D0%B0/%D0%BF%D1%8A%D1%82%D0%BD%D0%B8%D1%82%D0%B5-%D0%B8%D0%BD%D1%86%D0%B8%D0%B4%D0%B5%D0%BD%D1%82%D0%B8-%D0%BF%D1%80%D0%B5%D0%B7-%D0%B8%D0%B7%D0%BC%D0%B8%D0%BD%D0%B0%D0%BB%D0%BE%D1%82%D0%BE-%D0%B4%D0%B5%D0%BD%D0%BE%D0%BD%D0%BE%D1%89%D0%B8%D0%B5-%D0%BE%D0%B1%D1%81%D1%82%D0%B0%D0%BD%D0%BE%D0%B2%D0%BA%D0%B0%D1%82%D0%B0-%D0%BF%D0%BE-%D0%BF%D1%8A%D1%82%D0%B8%D1%89%D0%B0%D1%82%D0%B002012019";
            var provider = new MvrBgPutnaObstanovkaSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Пътните инциденти през изминалото денонощие; обстановката по пътищата", news.Title);
            Assert.Contains("Динамичните данни са на база текущи съобщения, получени от ОДМВР до 24:00 часа на 31 януари 2019", news.Content);
            Assert.Contains("Шофьорите да се движат с повишено внимание по тези участъци.", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2019, 2, 1, 9, 5, 0), news.PostDate);
            Assert.Equal("пътна-обстановка/пътните-инциденти-през-изминалото-денонощие-обстановката-по-пътищата02012019", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            foreach (var source in this.sources)
            {
                var result = source.GetLatestPublications();
                Assert.Equal(5, result.Count());
            }
        }
    }
}
