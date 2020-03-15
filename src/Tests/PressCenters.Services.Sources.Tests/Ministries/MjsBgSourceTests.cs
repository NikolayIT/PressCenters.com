namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class MjsBgSourceTests
    {
        [Theory]
        [InlineData("https://mjs.bg/home/index/6136e040-2cfa-45be-84c1-fb4b8674c352", "6136e040-2cfa-45be-84c1-fb4b8674c352")]
        [InlineData("https://mjs.bg/home/index/5cfdb35b-76b7-4848-9444-0ae4183ccd46/", "5cfdb35b-76b7-4848-9444-0ae4183ccd46")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new MjsBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://mjs.bg/home/index/1eac25bf-981b-4487-a505-593e11e56ed6";
            var provider = new MjsBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министър Кирилов участва в Националната програма „Управленски умения”", news.Title);
            Assert.Contains("Министър Данаил Кирилов запозна млади лидери от парламентарно представените партии,", news.Content);
            Assert.Contains("изслушване на малолетни и непълнолетни по граждански и наказателни производства.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("2020-02-10", news.Content);
            //// Assert.Equal(new DateTime(2020, 2, 10), news.PostDate);
            Assert.Equal("https://mjs.bg/api/part/GetBlob?hash=AC9F6B976DF04C6B609471615B53F679", news.ImageUrl);
            Assert.Equal("1eac25bf-981b-4487-a505-593e11e56ed6", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new MjsBgSource();
            var result = provider.GetLatestPublications().ToList();
            Assert.Equal(5, result.Count());
        }
    }
}
