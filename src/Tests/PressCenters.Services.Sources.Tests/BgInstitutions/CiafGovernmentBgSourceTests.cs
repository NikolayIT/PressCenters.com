namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CiafGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.caciaf.bg/aktualno/novini/administrativni-sydili-sht-a-potvyrdiha-re-sh-eni-ja-na-kpkonpi-za-konflikt-na-interesi/", "administrativni-sydili-sht-a-potvyrdiha-re-sh-eni-ja-na-kpkonpi-za-konflikt-na-interesi")]
        [InlineData("https://www.caciaf.bg/aktualno/novini/kpkonpi-vnas-ja-iskove-za-otnemane-na-imu-sht-estvo-na-stojnost-nad-7-mln-lv", "kpkonpi-vnas-ja-iskove-za-otnemane-na-imu-sht-estvo-na-stojnost-nad-7-mln-lv")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new CiafGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.caciaf.bg/aktualno/novini/zapo-ch-vat-studentski-stajove-po-iniciativa-akademi-ja-antikorupci-ja";
            var provider = new CiafGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Започват Студентски Стажове По Инициатива „Академия Антикорупция“", news.Title);
            Assert.Equal("zapo-ch-vat-studentski-stajove-po-iniciativa-akademi-ja-antikorupci-ja", news.RemoteId);
            Assert.Equal(new DateTime(2020, 10, 1).Date, news.PostDate.Date);
            Assert.Contains("Днес, 1 октомври 2020 г., председателят на Комисията за борба", news.Content);
            Assert.Contains("Предвижда се стажът на първата група студенти да започне на 1 ноември 2020 г.", news.Content);
            Assert.DoesNotContain("thumb_1008x437_size_800x600_5f75af76821ef", news.Content);
            Assert.DoesNotContain("Принтирай", news.Content);
            Assert.DoesNotContain("Изпрати на имейл", news.Content);
            Assert.Equal("https://www.caciaf.bg/web/files/news/519/main_image/thumb_1008x437_size_800x600_5f75af76821ef.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithoutImage()
        {
            const string NewsUrl = "https://www.caciaf.bg/aktualno/novini/na-vnimanieto-na-chlenovete-na-cik";
            var provider = new CiafGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("На Вниманието На Членовете На Цик", news.Title);
            Assert.Equal("na-vnimanieto-na-chlenovete-na-cik", news.RemoteId);
            Assert.Equal(new DateTime(2021, 6, 10).Date, news.PostDate.Date);
            Assert.Contains("КПКОНПИ прие решение /протокол № 998 от 19.05.2021", news.Content);
            Assert.Contains("от членовете на ЦИК се счита датата на назначаване с Указ № 131/12.05.2021 г.", news.Content);
            Assert.DoesNotContain("placeholder-1008x437", news.Content);
            Assert.DoesNotContain("Принтирай", news.Content);
            Assert.DoesNotContain("Изпрати на имейл", news.Content);
            Assert.Equal("https://www.caciaf.bg/web/frontend/images/placeholder/placeholder-1008x437.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CiafGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
