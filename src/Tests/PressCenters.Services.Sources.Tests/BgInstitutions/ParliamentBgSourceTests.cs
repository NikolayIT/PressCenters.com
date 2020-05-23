namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class ParliamentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.parliament.bg/bg/news/ID/4623", "4623")]
        [InlineData("https://www.parliament.bg/bg/news/ID/1", "1")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ParliamentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.parliament.bg/bg/news/ID/4624";
            var provider = new ParliamentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Цяла година Пловдив ще бъде център на духовността, на изкуството и на красотата, отбеляза в Пловдив председателят на парламента Цвета Караянчева на откриването на изложбата „Изкуството на свободата – от Берлинската стена до уличното изкуство“", news.Title);
            Assert.Contains("Цяла година Пловдив ще бъде център на духовността, на изкуството и на красотата", news.Content);
            Assert.Contains("на официалната церемония по откриването на \"Пловдив – Европейска столица на културата 2019\".", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("201901120811071", news.Content);
            Assert.DoesNotContain("12/01/2019", news.Content);
            Assert.Equal("https://www.parliament.bg/pub/Gallery/201901120811071.JPG", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 12), news.PostDate);
            Assert.Equal("4624", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.parliament.bg/bg/news/ID/2";
            var provider = new ParliamentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Проф. Герджиков се срещна с представители на Медийната обсерватория", news.Title);
            Assert.Contains("Председателят на Народното събрание проф. Огнян Герджиков се срещна представители на Медийната обсерватория – България.", news.Content);
            Assert.Contains("Предложенията ще бъдат разпределени в парламентарната медийна комисия.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("30/05/2002", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2002, 5, 30), news.PostDate);
            Assert.Equal("2", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new ParliamentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(3, result.Count());
        }
    }
}
