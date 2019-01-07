namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MoewGovernmentBgSourcesTests
    {
        [Theory]
        [InlineData("https://www.moew.government.bg/bg/ministur-dimov-uchastva-v-poslednoto-za-godinata-zasedanie-na-suveta-na-es-po-okolna-sreda/", "ministur-dimov-uchastva-v-poslednoto-za-godinata-zasedanie-na-suveta-na-es-po-okolna-sreda")]
        [InlineData("https://www.moew.government.bg/bg/otnovo-sa-izmereni-stojnosti-na-vredni-vestestva-nad-normata-ot-mk-kremikovci-ad", "otnovo-sa-izmereni-stojnosti-na-vredni-vestestva-nad-normata-ot-mk-kremikovci-ad")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var sources = new List<BaseSource>()
                          {
                              new MoewGovernmentBgNationalNewsSource(),
                              new MoewGovernmentBgRegionalNewsSource(),
                          };

            foreach (var source in sources)
            {
                var result = source.ExtractIdFromUrl(url);
                Assert.Equal(id, result);
            }
        }

        [Fact]
        public void ParseNationalNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.moew.government.bg/bg/ministur-dimov-nyama-previshenie-na-normite-na-tejki-metali-vuv-vodite-na-reka-dragovistica/";
            var provider = new MoewGovernmentBgNationalNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Димов: Няма превишение на нормите на тежки метали във водите на река Драговищица", news.Title);
            Assert.Contains("Няма превишение на нормите на тежки метали във водите на река Драговищица.", news.Content);
            Assert.Contains("извършвана оценка за въздействието на околната среда и водите, нито за постъпвала нотификация по чл.3 от Конвенцията за ОВОС.", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("28 декември 2018 | 13:55", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.Equal("https://www.moew.government.bg/static/media/ups/cached/25a8beb4de8c140dca7155e300be9e4bf55f2fd3.JPG", news.ImageUrl);
            Assert.Equal(new DateTime(2018, 12, 28, 13, 55, 0), news.PostDate);
            Assert.Equal("ministur-dimov-nyama-previshenie-na-normite-na-tejki-metali-vuv-vodite-na-reka-dragovistica", news.RemoteId);
        }

        [Fact]
        public void ParseRegionalNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.moew.government.bg/bg/prirodnata-zabelejitelnost-gurbavata-cheshma-na-teritoriyata-na-riosv-shumen-e-spasena-ot-razrushavane/";
            var provider = new MoewGovernmentBgRegionalNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Природната забележителност “Гърбавата Чешма” на територията на РИОСВ- Шумен е спасена от разрушаване", news.Title);
            Assert.Contains("Предприети са спешни мерки за спасяването на природната забележителност “Гърбавата Чешма”", news.Content);
            Assert.Contains("а “Гърбавата чешма” и “Костадин тепе” са в община Антоново.", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("03 януари 2008 | 11:33", news.Content);
            Assert.DoesNotContain("facebook", news.Content);
            Assert.Equal("https://www.moew.government.bg/static/media/ups/cached/c89fad7513c67beba458b5c19a96ab7e26687cdf.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2008, 1, 3, 11, 33, 0), news.PostDate);
            Assert.Equal("prirodnata-zabelejitelnost-gurbavata-cheshma-na-teritoriyata-na-riosv-shumen-e-spasena-ot-razrushavane", news.RemoteId);
        }

        [Fact]
        public void GetNationalNewsShouldReturnResults()
        {
            var provider = new MoewGovernmentBgNationalNewsSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(12, result.Count());
        }

        [Fact]
        public void GetRegionalNewsShouldReturnResults()
        {
            var provider = new MoewGovernmentBgRegionalNewsSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(12, result.Count());
        }
    }
}
