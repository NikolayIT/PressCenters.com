namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MzhGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("http://www.mzh.government.bg/bg/press-center/novini/ot-dnes-stopanite-dokazvat-realizaciyata-na-plodov/", "novini/ot-dnes-stopanite-dokazvat-realizaciyata-na-plodov")]
        [InlineData("http://www.mzh.government.bg/bg/press-center/novini/prevedeni-sa-nad-634-mln-leva-po-shemata-za-edinno", "novini/prevedeni-sa-nad-634-mln-leva-po-shemata-za-edinno")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MzhGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.mzh.government.bg/bg/press-center/novini/masirani-proverki-za-nezakonen-drvodobiv-i-po-praz";
            var provider = new MzhGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Масирани проверки за незаконен дърводобив и по празниците", news.Title);
            Assert.Contains("„Решени сме да респектираме нарушителите и ограничим набезите в горите по празниците.", news.Content);
            Assert.Contains("Съставени са 8 акта и по един от случаите е образувано досъдебно производство.", news.Content);
            Assert.DoesNotContain("jpg__623x416_q85_crop_subsampling", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("21 Декември 2018", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.Equal(new DateTime(2018, 12, 21, 15, 17, 24), news.PostDate);
            Assert.Equal("http://www.mzh.government.bg/media/filer_public_thumbnails/filer_public/2018/12/21/forest-2m.jpg__623x416_q85_crop_subsampling-2_upscale.jpg", news.ImageUrl);
            Assert.Equal("novini/masirani-proverki-za-nezakonen-drvodobiv-i-po-praz", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithMultipleImagesShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.mzh.government.bg/bg/press-center/novini/4519-zamestnik-ministr-georgi-kostov-napravi-inspe/";
            var provider = new MzhGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Заместник-министър Георги Костов направи инспекция на изграждането на пограничните оградни съоръжения", news.Title);
            Assert.Contains("Заместник-министърът на земеделието и храните доц. Георги Костов", news.Content);
            Assert.Contains("за изграждането на съоръжението и е според заложените срокове от строителните фирми-изпълнителки.", news.Content);
            Assert.DoesNotContain("23092016_kostov_inspekcia_", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("23 Септември 2016", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.Equal(new DateTime(2016, 9, 23, 18, 59, 56), news.PostDate);
            Assert.Equal("http://www.mzh.government.bg/media/filer_public_thumbnails/filer_public/2018/02/23/23092016_kostov_inspekcia_.jpg__623x416_q85_crop_subsampling-2_upscale.jpg", news.ImageUrl);
            Assert.Equal("novini/4519-zamestnik-ministr-georgi-kostov-napravi-inspe", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.mzh.government.bg/bg/press-center/novini/70bb-ministr-naidenov-poseti-sizp-tsentra-vv-vrats/";
            var provider = new MzhGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Найденов посети СИЗП центъра във Враца", news.Title);
            Assert.Contains("Министърът на земеделието и храните д-р Мирослав Найденов", news.Content);
            Assert.Contains("на бази данни на СИЗП в Разград.", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("21 Декември 2010", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.Equal(new DateTime(2010, 12, 21, 14, 19, 42), news.PostDate);
            Assert.Equal("/images/sources/mzh.government.bg.png", news.ImageUrl);
            Assert.Equal("novini/70bb-ministr-naidenov-poseti-sizp-tsentra-vv-vrats", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MzhGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
