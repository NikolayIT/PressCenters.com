namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class ConstCourtBgSourceTests
    {
        [Theory]
        [InlineData("https://www.constcourt.bg/bg/news-1124", "news-1124")]
        [InlineData("https://www.constcourt.bg/bg/messages-1167", "messages-1167")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ConstCourtBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.constcourt.bg/bg/news-1178";
            var provider = new ConstCourtBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Днес, 27.06.2023 г., бе публикувана невярна информация в публичното пространство относно акт на Конституционния съд", news.Title);
            Assert.Equal("news-1178", news.RemoteId);
            Assert.Equal(new DateTime(2023, 6, 27), news.PostDate.Date);
            Assert.Contains("По повод разпространявана днес от някои медии невярна информация, Конституционният съд уточнява", news.Content);
            Assert.Contains("Определенията са приети единодушно.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
            Assert.DoesNotContain("2023-06-27", news.Content);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithImage()
        {
            const string NewsUrl = "https://www.constcourt.bg/bg/news-1124";
            var provider = new ConstCourtBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Председателят на Конституционния съд Павлина Панова взе участие в ХХХ конгрес на Международната федерация по Европейско право", news.Title);
            Assert.Equal("news-1124", news.RemoteId);
            Assert.Equal(new DateTime(2023, 6, 5), news.PostDate.Date);
            Assert.Contains("През изминалата седмица София бе домакин на XXX ĸoнгpec нa Meждyнapoднaтa фeдepaция пo Eвpoпeйcĸo пpaвo (FІDЕ).", news.Content);
            Assert.Contains("заместник-председателят на Европейската комисия Вера Юрова.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("https://www.constcourt.bg//content/news/1688722045ПРЕДСЕДАТЕЛЯТ НА КОНСТИТУЦИОННИЯ СЪД ПАВЛИНА ПАНОВА ВЗЕ УЧАСТИЕ В ХХХ КОНГРЕС НА МЕЖДУНАРОДНАТА ФЕДЕРАЦИЯ ПО ЕВРОПЕЙСКО ПРАВО-1 - Copy.jpg", news.ImageUrl);
            Assert.DoesNotContain("2023-06-05", news.Content);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new ConstCourtBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(20, result.Count());
        }
    }
}
