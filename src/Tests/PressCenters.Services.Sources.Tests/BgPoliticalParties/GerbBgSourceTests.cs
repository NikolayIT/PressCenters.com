namespace PressCenters.Services.Sources.Tests.BgPoliticalParties
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources;
    using PressCenters.Services.Sources.BgPoliticalParties;

    using Xunit;

    public class GerbBgSourceTests
    {
        [Theory]
        [InlineData("http://gerb.bg/bg/news/detail-d_r_andrei_kovachev_nito_edno_ot_tvardeniyata_na_korneliya_ninova_po_temata_za_seta_ne_otgovarya_na_ist-43049.html", "43049")]
        [InlineData("http://gerb.bg/bg/news/detail-emil_radev_podari_bileti_za_3d_kino_na_decata_ot_dom_knyaginya_nadejda_vav_varna-43042.html", "43042")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new GerbBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://gerb.bg/bg/news/detail-ministri_ot_pravitelstvoto_na_boiko_borisov_predstaviha_upravlenskata_programa_na_gerb_pred_400_jite-43048.html";
            var provider = new GerbBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министри от правителството на Бойко Борисов представиха управленската програма на ГЕРБ пред 400 жители на 23 МИР София", news.Title);
            Assert.Equal("43048", news.RemoteId);
            Assert.Null(news.ShortContent);
            Assert.Equal(new DateTime(2017, 2, 9, 8, 0, 0), news.PostDate);
            Assert.Contains("Образованието, сигурността, справедливостта и доходите", news.Content);
            Assert.Contains("ГЕРБ трябва да имат самочувствие", news.Content);
            Assert.Contains("актуални въпроси, зададени от присъстващите.", news.Content);
            Assert.DoesNotContain("Министри от правителството на Бойко Борисов представиха управленската програма на ГЕРБ пред 400 жители на 23 МИР София", news.Content);
            Assert.DoesNotContain("09.02.2017", news.Content);
            Assert.DoesNotContain("&#60; Назад", news.Content);
            Assert.DoesNotContain("< Назад", news.Content);
            Assert.Equal("http://gerb.bg/files/thumbnails/news-43048-81652_detail_big.JPG", news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new GerbBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.News.Count() >= 12);
        }
    }
}
