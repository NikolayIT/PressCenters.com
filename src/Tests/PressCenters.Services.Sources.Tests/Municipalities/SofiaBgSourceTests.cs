namespace PressCenters.Services.Sources.Tests.Municipalities
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Municipalities;

    using Xunit;

    public class SofiaBgSourceTests
    {
        [Theory]
        [InlineData("https://www.sofia.bg/web/guest/news-archive-2016/-/asset_publisher/hultJKe9uK9Q/content/prez-mesec-uni-se-organizirat-cetiri-mobilni-punkta-za-priemane-na-opasni-otpad-ci?inheritRedirect=false&redirect=https%3A%2F%2Fwww.sofia.bg%3A443%2Fweb%2Fguest%2Fnews-archive-2016%3Fp_p_id%3D101_INSTANCE_hultJKe9uK9Q%26p_p_lifecycle%3D0%26p_p_state%3Dnormal%26p_p_mode%3Dview%26p_p_col_id%3Dcolumn-1%26p_p_col_count%3D1%26_101_INSTANCE_hultJKe9uK9Q_advancedSearch%3Dfalse%26_101_INSTANCE_hultJKe9uK9Q_keywords%3D%26_101_INSTANCE_hultJKe9uK9Q_delta%3D10%26p_r_p_564233524_resetCur%3Dfalse%26_101_INSTANCE_hultJKe9uK9Q_cur%3D19%26_101_INSTANCE_hultJKe9uK9Q_andOperator%3Dtrue", "prez-mesec-uni-se-organizirat-cetiri-mobilni-punkta-za-priemane-na-opasni-otpad-ci")]
        [InlineData("https://www.sofia.bg/web/guest/news/-/asset_publisher/1ZlMReQfODHE/content/zapocnaha-tekusi-remonti-na-osnovni-bulevardi-v-stolicna-obsina", "zapocnaha-tekusi-remonti-na-osnovni-bulevardi-v-stolicna-obsina")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new SofiaBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.sofia.bg/web/guest/news/-/asset_publisher/1ZlMReQfODHE/content/vladimir-danailov-vr-ci-priza-na-bait-za-naj-uspesen-b-lgarski-ikt-proekt";
            var provider = new SofiaBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Владимир Данаилов връчи приза на БАИТ за най-успешен български ИКТ проект", news.Title);
            Assert.Equal("vladimir-danailov-vr-ci-priza-na-bait-za-naj-uspesen-b-lgarski-ikt-proekt", news.RemoteId);
            Assert.Equal(new DateTime(2020, 2, 27), news.PostDate);
            Assert.Contains("Столичният заместник-кмет по дигитализация, иновации и икономическо развитие Владимир Данаилов", news.Content);
            Assert.Contains("Всички финалисти може да видите тук", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("27.02.2020", news.Content);
            Assert.Equal("https://www.sofia.bg/documents/20182/7258816/2020-02-27-български+ИКТ+проект1.jpg/f7359720-2ca8-4109-bcaf-f380b5a937bc?t=1582788101589", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.sofia.bg/web/guest/news-archive-2016/-/asset_publisher/hultJKe9uK9Q/content/fand-kova-naj-vaznite-investicii-sa-tezi-koito-podobravat-kacestvoto-na-v-zduha-i-opazvat-okolnata-sreda";
            var provider = new SofiaBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Фандъкова: Най-важните инвестиции са тези, които подобряват качеството на въздуха и опазват околната среда", news.Title);
            Assert.Equal("fand-kova-naj-vaznite-investicii-sa-tezi-koito-podobravat-kacestvoto-na-v-zduha-i-opazvat-okolnata-sreda", news.RemoteId);
            Assert.Equal(new DateTime(2016, 4, 21), news.PostDate);
            Assert.Contains("Най-важните инвестиции са тези, които подобряват качеството на въздуха", news.Content);
            Assert.Contains("да изведем част от трафика чрез вътрешен градски ринг и околовръстен път.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("21.04.2016", news.Content);
            Assert.Equal("/images/sources/sofia.bg.png", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new SofiaBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }
    }
}
