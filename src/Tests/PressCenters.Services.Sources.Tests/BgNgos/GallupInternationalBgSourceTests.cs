namespace PressCenters.Services.Sources.Tests.BgNgos
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgNgos;

    using Xunit;

    public class GallupInternationalBgSourceTests
    {
        [Theory]
        [InlineData("http://www.gallup-international.bg/41142/gallup-international-is-a-partner-in-a-key-project-for-dual-education/", "41142")]
        [InlineData("http://www.gallup-international.bg/41107/electoral-profiles-general-elections-2019", "41107")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new GallupInternationalBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.gallup-international.bg/41285/attitudes-towards-democracy/";
            var provider = new GallupInternationalBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("По света харесват демокрацията. У нас също я харесваме, но по-малко. Вярата в демокрацията изглежда в опасност.", news.Title);
            Assert.Contains("76% от хората по света се съгласяват с твърдението, че демокрацията може да има недостатъци, но е най-добрата форма на управление", news.Content);
            Assert.Contains("България е в съзвучие със страните в Източна Европа, които споделят по-скоро позитивни оценки за демокрацията, на не в особено висока степен.", news.Content);
            Assert.Contains("Ако у нас наистина убедеността в качеството на демокрацията трайно спада, въпросът следва да получи адекватно внимание, защото е повод за тревога.", news.Content);
            Assert.DoesNotContain("Вътрешна политика", news.Content);
            Assert.DoesNotContain("26 юни 2019", news.Content);
            Assert.Equal("http://www.gallup-international.bg/files/2019/02/vote-3569999_1280.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 6, 26, 10, 17, 17), news.PostDate);
            Assert.Equal("41285", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new GallupInternationalBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
