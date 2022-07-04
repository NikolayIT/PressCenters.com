namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class FscBgSourceTests
    {
        [Theory]
        [InlineData("https://www.fsc.bg/?p=42621", "42621")]
        [InlineData("https://www.fsc.bg/?p=30839", "30839")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new FscBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.fsc.bg/?p=42318";
            var provider = new FscBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Окончателни резултати от дейността по допълнително пенсионно осигуряване за 2021 г.", news.Title);
            Assert.Equal("42318", news.RemoteId);
            Assert.Equal(new DateTime(2022, 5, 3).Date, news.PostDate.Date);
            Assert.Contains("Управление “Осигурителен надзор” на КФН обяви", news.Content);
            Assert.Contains("в раздел: Пазари / Осигурителен пазар / Статистика / Статистика и анализи / 2021.", news.Content);
            Assert.True(!news.Content.Contains(news.Title));
            Assert.True(!news.Content.Contains("03.05.2022"));
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.fsc.bg/?p=44209";
            var provider = new FscBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Комисията за финансов надзор с участие в научно-приложната конференция „Икономика на страха“", news.Title);
            Assert.Equal("44209", news.RemoteId);
            Assert.Equal(new DateTime(2022, 6, 27).Date, news.PostDate.Date);
            Assert.Contains("Висшето училище по застраховане и финанси (ВУЗФ) и Лабораторията за научно-приложни изследвания към него проведоха", news.Content);
            Assert.Contains("ще продължи да подкрепя бъдещите кръгли маси, уебинари, конференции и тематични професионални дискусии.", news.Content);
            Assert.True(!news.Content.Contains(news.Title));
            Assert.True(!news.Content.Contains(news.ImageUrl));
            Assert.True(!news.Content.Contains("27.06.2022"));
            Assert.Equal("https://www.fsc.bg/wp-content/uploads/2022/06/KSS_108-1024x683.jpg", news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new FscBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(10, result.Count());
        }
    }
}
