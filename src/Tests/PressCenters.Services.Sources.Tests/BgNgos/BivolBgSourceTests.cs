namespace PressCenters.Services.Sources.Tests.BgNgos
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgNgos;

    using Xunit;

    public class BivolBgSourceTests
    {
        [Theory]
        [InlineData("https://bivol.bg/%D0%BC%D0%B8%D0%BD%D0%B8%D1%81%D1%82%D1%8A%D1%80%D1%8A%D1%82-%D0%BD%D0%B0-%D1%84%D0%B8%D0%BD%D0%B0%D0%BD%D1%81%D0%B8%D1%82%D0%B5-%D0%BD%D0%B0-%D0%BC%D0%BE%D0%BB%D0%B4%D0%BE%D0%B2%D0%B0-%D0%B8%D1%81.html", "министърът-на-финансите-на-молдова-ис")]
        [InlineData("https://bivol.bg/dyuni-ama-sled-vek.html", "dyuni-ama-sled-vek")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new BivolBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://bivol.bg/bombardier-gpgroup-nkzhi.html";
            var provider = new BivolBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Bombardier и “Джи Пи Груп” спечелиха жп-търг за близо 200 милиона с измама", news.Title);
            Assert.Contains("Консорциум между дъщерни дружества на канадския&nbsp; технологичен и машиностроителен гигант", news.Content);
            Assert.Contains("олигархичният модел #КОЙ печели при всички случаи.", news.Content);
            Assert.DoesNotContain("Благодарим Ви, че четете Биволъ", news.Content);
            Assert.DoesNotContain("Данъкъ Биволъ", news.Content);
            Assert.DoesNotContain("януари 29, 2019", news.Content);
            Assert.DoesNotContain("PDF", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("https://bivol.bg/wp-content/uploads/2018/12/nkzhi.png", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 29, 16, 5, 57), news.PostDate);
            Assert.Equal("bombardier-gpgroup-nkzhi", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithAuthorShouldWorkCorrectly()
        {
            const string NewsUrl = "https://bivol.bg/2010-10-30-22-28-29-17.html";
            var provider = new BivolBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Информацията е най-страшният враг на властта", news.Title);
            Assert.Contains("Журналистите въоръжени със скрита камера обикалят министерства, кметства и архиви", news.Content);
            Assert.Contains("сами с властта за правото на информация, никога и нищо няма да получат от нея спонтанно.", news.Content);
            Assert.DoesNotContain("Благодарим Ви, че четете Биволъ", news.Content);
            Assert.DoesNotContain("Данъкъ Биволъ", news.Content);
            Assert.DoesNotContain("септември 7, 2005", news.Content);
            Assert.DoesNotContain("PDF", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("/images/sources/bivol.bg.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2005, 9, 7, 12, 9, 0), news.PostDate);
            Assert.Equal("2010-10-30-22-28-29-17", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new BivolBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
