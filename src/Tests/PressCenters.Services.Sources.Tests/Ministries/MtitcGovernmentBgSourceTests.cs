namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MtitcGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mtc.government.bg/bg/category/1/ministur-rosen-zhelyazkov-zasilvame-kontrola-po-vreme-na-praznicite", "1/ministur-rosen-zhelyazkov-zasilvame-kontrola-po-vreme-na-praznicite")]
        [InlineData("https://www.mtc.government.bg/bg/category/1/otkrivane-na-noviya-trenazhoren-kompleks-za-podgotovka-na-rukovoditeli-na-poleti-na-dp-rukovodstvo-na-vuzdushnoto-dvizhenie/", "1/otkrivane-na-noviya-trenazhoren-kompleks-za-podgotovka-na-rukovoditeli-na-poleti-na-dp-rukovodstvo-na-vuzdushnoto-dvizhenie")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MtitcGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mtc.government.bg/bg/category/1/blgariya-i-rumniya-izgradikha-avariyno-spasitelni-centrove-po-reka-dunav";
            var provider = new MtitcGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("България и Румъния изградиха аварийно-спасителни центрове по река Дунав", news.Title);
            Assert.Contains("Успешно приключи проектът „Повишаване на транспортната безопасност в общия българо-румънски участък на река Дунав", news.Content);
            Assert.Contains("Общият бюджет на проекта е 5 699 612 евро, от които 85% се предоставят от Европейския фонд за регионално развитие, а останалите 15% са национално съфинансиране.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("1.jpg", news.Content);
            Assert.DoesNotContain("07.10.2022", news.Content);
            Assert.DoesNotContain("facebook.com", news.Content);
            Assert.DoesNotContain("gallery", news.Content);
            Assert.Equal("https://www.mtc.government.bg/sites/default/files/images/2022-10/1.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2022, 10, 7, 13, 5, 48), news.PostDate);
            Assert.Equal("1/blgariya-i-rumniya-izgradikha-avariyno-spasitelni-centrove-po-reka-dunav", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MtitcGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
