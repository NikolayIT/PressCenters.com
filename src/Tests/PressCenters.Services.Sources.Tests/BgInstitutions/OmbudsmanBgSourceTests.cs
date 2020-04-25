namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class OmbudsmanBgSourceTests
    {
        [Theory]
        [InlineData("https://www.ombudsman.bg/news/1", "1")]
        [InlineData("https://www.ombudsman.bg/news/2/", "2")]
        [InlineData("https://www.ombudsman.bg/news/4984#middleWrapper", "4984")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new OmbudsmanBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.ombudsman.bg/news/4979#middleWrapper";
            var provider = new OmbudsmanBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Омбудсманът Мая Манолова представи законопроект за ограничаване на привилегиите на банки и монополисти", news.Title);
            Assert.Contains("Омбудсманът Мая Манолова представи днес на обществена дискусия в институцията законопроект за изменения в Гражданския процесуален кодекс", news.Content);
            Assert.Contains("Видео от изказването", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.Equal("https://www.ombudsman.bg//pictures/image.php?name=DSC_0028.JPG&w=242", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 2, 25), news.PostDate);
            Assert.Equal("4979", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.ombudsman.bg/news/9";
            var provider = new OmbudsmanBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Омбудсманът на Република България предлага промени в законопроекта за Общ устройствен план на София", news.Title);
            Assert.Contains("Гиньо Ганев представи на председателя на Народното събрание Георги Пирински предложение", news.Content);
            Assert.Contains("представители на специализираните архитектурните среди и на Министерството на регионалното развитие и благоустройството.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2006, 10, 4), news.PostDate);
            Assert.Equal("9", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new OmbudsmanBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
