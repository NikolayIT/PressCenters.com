namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class OmbudsmanBgSourceTests
    {
        [Theory]
        [InlineData("https://www.ombudsman.bg/?go=news&p=detail&newsId=2919", "2919")]
        [InlineData("https://www.ombudsman.bg/?go=news&p=detail&newsId=2912", "2912")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new OmbudsmanBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.ombudsman.bg/?go=news&p=detail&newsId=2919";
            var provider = new OmbudsmanBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.NotNull(news);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Омбудсманът връчи специална награда на „Лекар на годината“", news.Title);
            Assert.Contains("Омбудсманът Велислава Делчева връчи специалната награда", news.Content);
            Assert.Contains("ръководител на Простата центъра в УМБАЛ", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("Още новини", news.Content);
            Assert.Equal("https://www.ombudsman.bg/?h=newsphoto&newsphotoId=4799", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 10, 20), news.PostDate);
            Assert.Equal("2919", news.RemoteId);
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
