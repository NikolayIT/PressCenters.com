namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class ModBgSourceTests
    {
        [Theory]
        [InlineData("https://www.mod.bg/bg/news_archive.php?fn_month=1&fn_year=2019#10496", "10496")]
        [InlineData("https://www.mod.bg/bg/news.php#10484", "10484")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new ModBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mod.bg/bg/news_archive.php?fn_month=1&fn_year=2019#10498";
            var provider = new ModBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Министърът на отбраната Красимир Каракачанов награди военни разузнавачи по повод празника на служба „Военна информация“", news.Title);
            Assert.Contains("111-ата годишнина от създаването на българското военно разузнаване отбеляза служба „Военна информация“ ", news.Content);
            Assert.Contains("12 януари е обявен за официален празник на българското военно разузнаване.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("11.01.2019", news.Content);
            Assert.DoesNotContain("uploads/01_1", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Equal("http://www.mod.bg/bg/fn/uploads/01_1.JPG", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 11), news.PostDate);
            Assert.Equal("10498", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.mod.bg/bg/news_archive.php?fn_month=4&fn_year=2011#1403";
            var provider = new ModBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Няма пострадали български военнослужещи при атака на база „Феникс” в Кабул, Афганистан", news.Title);
            Assert.Contains("Няма пострадали български военнослужещи при атака днес, 2 април", news.Content);
            Assert.Contains("Кабул е подложена на комбинирана атака с леко стрелково оръжие, ръчен гранатомет и атентатори самоубийци.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("02.04.2011", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.Equal("/images/sources/mod.bg.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2011, 4, 2), news.PostDate);
            Assert.Equal("1403", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new ModBgSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.Any());
        }
    }
}
