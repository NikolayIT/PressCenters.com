namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MvrBgSourceTests
    {
        private readonly List<BaseSource> sources = new List<BaseSource>
        {
            new MvrBgAktualnoSource(),
            new MvrBgNoviniSource(),
            new MvrBgInformacionenBiuletinSource(),
            new MvrBgPutnaObstanovkaSource(),
        };

        [Theory]
        [InlineData("https://www.mvr.bg/press/актуална-информация/актуална-информация/новини/91393", "новини/91393")]
        [InlineData("https://www.mvr.bg/press/актуална-информация/актуална-информация/актуално/100101_01", "актуално/100101_01")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            foreach (var source in this.sources)
            {
                var result = source.ExtractIdFromUrl(url);
                Assert.Equal(id, result);
            }
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mvr.bg/press/актуална-информация/актуална-информация/новини/91393";
            var provider = new MvrBgNoviniSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Гранични полицаи задържаха дрогиран шофьор без книжка с 27 незаконни мигранти", news.Title);
            Assert.Contains("Аркутино", news.Content);
            Assert.Contains("мигранти", news.Content);
            Assert.Equal("https://www.mvr.bg/upload/296384/768x432.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 6, 20), news.PostDate);
            Assert.Equal("новини/91393", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            foreach (var source in this.sources)
            {
                var result = source.GetLatestPublications();
                Assert.Equal(5, result.Count());
            }
        }
    }
}
