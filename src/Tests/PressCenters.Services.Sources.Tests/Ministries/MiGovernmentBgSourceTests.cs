namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MiGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mi.government.bg/bg/news/prodaljava-obshtestvenoto-obsajdane-na-naredbata-za-licata-osashtestvyavashti-targoviya-s-neft-i-nefteni-pro-3632.html", "3632")]
        [InlineData("https://www.mi.government.bg/bg/news/i-n-f-o-r-m-a-c-i-ya-za-provedeno-zasedanie-na-nacionalniya-ikonomicheski-savet-3469.html?p=eyJwYWdlIjoxMH0=", "3469")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MiGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mi.government.bg/bg/news/ministar-karanikolov-germanska-kompaniya-shte-investira-21-5-mln-lv-v-zavod-krai-vraca-3633.html?p=eyJwYWdlIjoxfQ==";
            var provider = new MiGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Караниколов: Германска компания ще инвестира 21,5 млн. лв. в завод край Враца", news.Title);
            Assert.Contains("Министърът на икономиката Емил Караниколов връчи сертификат за инвестиция клас А на", news.Content);
            Assert.Contains("производители на части и компоненти за автомобили“, поясни Караниколов.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("default_news-3633", news.Content);
            Assert.DoesNotContain("separator", news.Content);
            Assert.DoesNotContain("03 януари 2019", news.Content);
            Assert.DoesNotContain("отпечатай тази страница", news.Content);
            Assert.DoesNotContain("обратно в списъка", news.Content);
            Assert.Equal(new DateTime(2019, 1, 3), news.PostDate);
            Assert.Equal("https://www.mi.government.bg/files/news/image/default_news-3633-5586.jpg", news.ImageUrl);
            Assert.Equal("3633", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mi.government.bg/bg/news/i-n-f-o-r-m-a-c-i-ya-za-provedeno-zasedanie-na-nacionalniya-ikonomicheski-savet-3545.html?p=eyJwYWdlIjoxMH0=";
            var provider = new MiGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("И Н Ф О Р М А Ц И Я за проведено заседание на Националния икономически съвет", news.Title);
            Assert.Contains("На 10.09.2018 г. от 14.00 часа в сградата на Министерство на икономиката, ул. „Славянска“ № 8", news.Content);
            Assert.Contains("сключване на търговски споразумения, които касаят българския бизнес.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("separator", news.Content);
            Assert.DoesNotContain("12 септември 2018", news.Content);
            Assert.DoesNotContain("отпечатай тази страница", news.Content);
            Assert.DoesNotContain("обратно в списъка", news.Content);
            Assert.Equal(new DateTime(2018, 9, 12), news.PostDate);
            Assert.Null(news.ImageUrl);
            Assert.Equal("3545", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MiGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
