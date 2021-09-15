namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class VssJusticeBgSourceTests
    {
        [Theory]
        [InlineData("http://www.vss.justice.bg/page/view/107313", "107313")]
        [InlineData("http://www.vss.justice.bg/page/view/3027", "3027")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new VssJusticeBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.vss.justice.bg/page/view/2995";
            var provider = new VssJusticeBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Прессъобщение", news.Title);
            Assert.Contains("На 13.07.2015 г. в сградата на Висшия съдебен съвет се проведе съвместно заседание", news.Content);
            Assert.Contains("с оглед завършването на процедурите по атестиране.", news.Content);
            Assert.DoesNotContain(news.ImageUrl, news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Equal("http://www.vss.justice.bg/root/f/upload/8/13-07-2015-1-1.jpg", news.ImageUrl);
            Assert.Equal("2995", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.vss.justice.bg/page/view/107318";
            var provider = new VssJusticeBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Декларация на Прокурорската колегия на Висшия съдебен съвет", news.Title);
            Assert.Contains("по повод препоръка&nbsp;на Временната комисия за разследване на факти и обстоятелства", news.Content);
            Assert.Contains("Висшия съдебен съвет по Протокол № 9 от 20 март 2019 г.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal("107318", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new VssJusticeBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
