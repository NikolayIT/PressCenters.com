namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class OmbudsmanBgSourceTests
    {
        [Theory]
        [InlineData("https://www.ombudsman.bg/bg/n/ombudsmanat-poiska-nezabavni-merki-za-pred-1877", "ombudsmanat-poiska-nezabavni-merki-za-pred-1877")]
        [InlineData("https://www.ombudsman.bg/bg/n/ombudsmanat-voda-v-svoge-veche-ima-no-vodna-1876/", "ombudsmanat-voda-v-svoge-veche-ima-no-vodna-1876")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new OmbudsmanBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.ombudsman.bg/bg/n/eksperti-na-ombudsmana-diana-kovacheva-dnes-1881";
            var provider = new OmbudsmanBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.NotNull(news);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Експерти на омбудсмана Диана Ковачева днес дават приемна за граждани в Пловдив", news.Title);
            Assert.Contains("Екип от експерти на омбудсмана Диана Ковачева ще консултират днес", news.Content);
            Assert.Contains("Търговище, Разград, Силистра, Шумен, Видин, Враца, Стара Загора, Сливен, Смолян и Пловдив", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.Equal("https://www.ombudsman.bg/storage//pub/gallery/20220831122734_Diana Kovatcheva.JPG", news.ImageUrl);
            Assert.Equal(new DateTime(2022, 8, 31), news.PostDate);
            Assert.Equal("eksperti-na-ombudsmana-diana-kovacheva-dnes-1881", news.RemoteId);
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
