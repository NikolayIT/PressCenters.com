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
            var result = provider.ExtractIdFromUrl("https://www.mvr.bg/press/актуална-информация/актуална-информация/новини/преглед/новини/създадена-е-организация-за-спокойното-протичане-на-идните-празнични-и-почивни-дни");
            Assert.Equal("създадена-е-организация-за-спокойното-протичане-на-идните-празнични-и-почивни-дни", result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mvr.bg/press/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/%D0%BF%D1%80%D0%B5%D0%B3%D0%BB%D0%B5%D0%B4/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/%D1%80%D1%8F%D0%B7%D0%BA%D0%B0%D1%82%D0%B0-%D0%BF%D1%80%D0%BE%D0%BC%D1%8F%D0%BD%D0%B0-%D0%BD%D0%B0-%D0%B2%D1%80%D0%B5%D0%BC%D0%B5%D1%82%D0%BE-%D0%BA%D1%80%D0%B8%D0%B5-%D0%BE%D0%BF%D0%B0%D1%81%D0%BD%D0%BE%D1%81%D1%82%D0%B8-%D0%BD%D0%B0-%D0%BF%D1%8A%D1%82%D1%8F";
            var provider = new MvrBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Рязката промяна на времето крие опасности на пътя", news.Title);
            Assert.Equal("Пътна полиция съветва: Поемайте на път добре подготвени", news.ShortContent);
            Assert.Contains("Времето осезателно се промени днес, за утре се очакват значителни валежи от дъжд в южните райони, и от сняг", news.Content);
            Assert.Contains("При попадане в рискова ситуация сигнализирайте първо на телефона за спешни случаи 112, след това потърсете за съдействие близки и приятели.", news.Content);
            Assert.DoesNotContain("https://www.mvr.bg/GetImage.ashx?id=d4f8176a-def9-42d8-8f24-ce0d3c503554&height=460&width=1260", news.Content);
            Assert.DoesNotContain("<script>", news.Content);
            Assert.DoesNotContain("glyphicon-edit", news.Content);
            Assert.DoesNotContain("<legend>Изображения</legend>", news.Content);
            Assert.Equal("https://www.mvr.bg/GetImage.ashx?id=d4f8176a-def9-42d8-8f24-ce0d3c503554&height=460&width=1260", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 11, 27), news.PostDate.Date);
            Assert.Equal("рязката-промяна-на-времето-крие-опасности-на-пътя", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mvr.bg/press/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%B0-%D0%B8%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D1%8F/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/%D0%BF%D1%80%D0%B5%D0%B3%D0%BB%D0%B5%D0%B4/%D0%B0%D0%BA%D1%82%D1%83%D0%B0%D0%BB%D0%BD%D0%BE/72-%D0%B3%D0%BE%D0%B4%D0%B8%D1%88%D0%B5%D0%BD-%D0%B5-%D0%BE%D1%82%D0%B2%D0%B5%D0%B4%D0%B5%D0%BD-%D0%B2-%D1%81%D1%82%D0%BE%D0%BB%D0%B8%D1%87%D0%BD%D0%BE%D1%82%D0%BE-%D0%BF%D1%8A%D1%80%D0%B2%D0%BE-%D1%80%D0%B0%D0%B9%D0%BE%D0%BD%D0%BD%D0%BE-%D1%83%D0%BF%D1%80%D0%B0%D0%B2%D0%BB%D0%B5%D0%BD%D0%B8%D0%B5";
            var provider = new MvrBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("72-годишен е отведен в столичното Първо районно управление", news.Title);
            Assert.Null(news.ShortContent);
            Assert.Contains("В късния следобед днес 72-годишен мъж дошъл на входа на Президентството и поискал среща.", news.Content);
            Assert.Contains("Образувано е досъдебно производство, изясняват се всички обстоятелства по случая.", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 3), news.PostDate);
            Assert.Equal("72-годишен-е-отведен-в-столичното-първо-районно-управление", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new MvrBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.News.Count() >= 8);
        }
    }
}
