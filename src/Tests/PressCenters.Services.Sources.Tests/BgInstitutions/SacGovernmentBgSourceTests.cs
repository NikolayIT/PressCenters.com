namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;
    using Xunit;

    public class SacGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://sac.justice.bg/novini/2026/vanya-ancheva-if-predsedatel-vas/", "vanya-ancheva-if-predsedatel-vas")]
        [InlineData("https://sac.justice.bg/novini/2026/kpk-vks-izbor-reshenie-7562-2026", "kpk-vks-izbor-reshenie-7562-2026")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new SacGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://sac.justice.bg/novini/2026/vanya-ancheva-if-predsedatel-vas/";
            var provider = new SacGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Съдия Ваня Анчева беше определена за изпълняващ функциите председател на Върховния административен съд от Пленума на ВСС", news.Title);
            Assert.StartsWith("<p>Председателят на Седмо отделение на Върховния административен съд съдия Ваня Анчева беше определена", news.Content);
            Assert.Contains("Общият й юридически стаж е над 38 години.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("Още от същата категория", news.Content);
            Assert.DoesNotContain("vanya-ancheva-1.webp", news.Content);
            Assert.Equal("https://sac.justice.bg/wp-content/uploads/2026/10/vanya-ancheva-1.webp", news.ImageUrl);
            Assert.Equal(new DateTime(2026, 10, 1), news.PostDate);
            Assert.Equal("vanya-ancheva-if-predsedatel-vas", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://sac.justice.bg/novini/2026/kpk-vks-izbor-reshenie-7562-2026/";
            var provider = new SacGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Кандидатът Ивайло Йосифов Иванов не е избран за член на КПК, постановява ВАС, с което потвърди решението на общото събрание на съдиите на ВКС", news.Title);
            Assert.Contains("Върховният административен съд отхвърли като неоснователна жалбата на Ивайло Йосифов Иванов от гр. Русе", news.Content);
            Assert.Contains("за действителни са приети 55 от общо 56 подадени бюлетини", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2026, 8, 21), news.PostDate);
            Assert.Equal("kpk-vks-izbor-reshenie-7562-2026", news.RemoteId);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new SacGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
