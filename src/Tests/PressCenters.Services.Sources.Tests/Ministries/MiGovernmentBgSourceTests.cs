namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MiGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mi.government.bg/news/otbelyazvame-50-godini-ot-navlizaneto-na-balgarskoto-kiselo-mlyako-na-yaponskiya-pazar-prez-2023-g/", "otbelyazvame-50-godini-ot-navlizaneto-na-balgarskoto-kiselo-mlyako-na-yaponskiya-pazar-prez-2023-g")]
        [InlineData("https://www.mi.government.bg/news/ministar-nikola-stoyanov-uchastva-v-godishnata-srestha-na-sdruzhenieto-na-targovczite-na-nehranitelni-stoki-u-nas", "ministar-nikola-stoyanov-uchastva-v-godishnata-srestha-na-sdruzhenieto-na-targovczite-na-nehranitelni-stoki-u-nas")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MiGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mi.government.bg/news/ministar-nikola-stoyanov-vsyaka-godina-v-balgariya-se-sazdavat-nad-30-hil-novi-malki-i-sredni-predpriyatiya/";
            var provider = new MiGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Никола Стоянов: Всяка година в България се създават над 30 хил. нови малки и средни предприятия", news.Title);
            Assert.Contains("Всяка година в България се създават над 30 хил. нови малки и средни предприятия.", news.Content);
            Assert.Contains("Събитието се организира за осма поредна година от в. „24 часа“.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("19.10.2022", news.Content);
            Assert.DoesNotContain("СПОДЕЛЕТЕ", news.Content);
            Assert.Equal(new DateTime(2022, 10, 19), news.PostDate);
            Assert.Equal("https://www.mi.government.bg/wp-content/uploads/2022/10/ministar-nikola-stoyanov-vsyaka-godina-v-balgariya-se-sazdavat-nad-30-hil.-novi-malki-i-sredni-predpriyatiya-1200x907.jpg", news.ImageUrl);
            Assert.Equal("ministar-nikola-stoyanov-vsyaka-godina-v-balgariya-se-sazdavat-nad-30-hil-novi-malki-i-sredni-predpriyatiya", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MiGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
