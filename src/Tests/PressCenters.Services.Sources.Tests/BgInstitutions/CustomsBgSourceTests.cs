namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class CustomsBgSourceTests
    {
        [Theory]
        [InlineData("https://customs.bg/wps/portal/agency/media-center/news-details/06-01-gorivo-sf", "news-details/06-01-gorivo-sf")]
        [InlineData("https://customs.bg/wps/portal/agency/media-center/on-focus/09-01-one-desk/", "on-focus/09-01-one-desk")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var sources = new List<BaseSource> { new CustomsBgNewsSource(), new CustomsBgOnFocusSource(), };
            foreach (var source in sources)
            {
                var result = source.ExtractIdFromUrl(url);
                Assert.Equal(id, result);
            }
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://customs.bg/wps/portal/agency/media-center/news-details/11-01-cigarettes-elin-pelin";
            var provider = new CustomsBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Над половин милион къса нелегални цигари, скрити в строителни панели, задържаха столични митничари", news.Title);
            Assert.Contains("549 600 къса нелегални цигари задържаха митнически служители от отдел", news.Content);
            Assert.Contains("По случая е образувано досъдебно производство под надзора на Районна прокуратура Елин Пелин.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("01.jpg", news.Content);
            Assert.DoesNotContain("11 януари 2019", news.Content);
            Assert.StartsWith("https://customs.bg/wps/wcm/connect/customs.bg28892/6a1fa44b-6fe1-48b5-97ab-a1a87bef4908/01.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 11), news.PostDate);
            Assert.Equal("news-details/11-01-cigarettes-elin-pelin", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithNewNews()
        {
            const string NewsUrl = "https://customs.bg/wps/portal/agency/media-center/news-details/11-01-22-Heroin_BG";
            var provider = new CustomsBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Митнически служители откриха близо 14,7 кг. хероин в тайник в лек автомобил при проверка в района на Дунав мост 2", news.Title);
            Assert.Contains("При проверка на кола с българска регистрация на ГКПП Дунав мост - Видин", news.Content);
            Assert.Contains("„задържане под стража“ спрямо обвиняемия И.П.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("01.JPG", news.Content);
            Assert.DoesNotContain("11 януари 2022", news.Content);
            Assert.StartsWith("https://customs.bg/wps/wcm/connect/customs.bg28892/c91f897a-c64a-4557-9c32-54ddcd082c24/01.JPG?MOD=AJPERES", news.ImageUrl);
            Assert.Equal(new DateTime(2022, 1, 11), news.PostDate);
            Assert.Equal("news-details/11-01-22-Heroin_BG", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://customs.bg/wps/portal/agency/media-center/news-details/2016-10-25";
            var provider = new CustomsBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Над 26 килограма кокаин задържаха митническите и гранични служители на Кулата", news.Title);
            Assert.Contains("26,113 килограма кокаин бяха задържани на ГКПП Кулата.", news.Content);
            Assert.Contains("Над 56 кг кокаин и 316 кг хероин са задържани от Агенция „Митници\" от началото на годината.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("25 октомври 2016", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2016, 10, 25), news.PostDate);
            Assert.Equal("news-details/2016-10-25", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteOnFocusNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "https://customs.bg/wps/portal/agency/media-center/on-focus/04-01-new-structura";
            var provider = new CustomsBgNewsSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Считано от 00.00 часа на 07.01.2019 г. влизат в сила промените в структурата на Агенция „Митници“", news.Title);
            Assert.Contains("Считано от 00.00 часа на 07.01.2019 г. в структурата на Агенция „Митници“", news.Content);
            Assert.Contains("на директора на Агенция „Митници“ (ДВ, бр. 74 от 2018 г.), публикувана в&nbsp; Държавен вестник, бр.2 от 2019 г.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("04 януари 2019", news.Content);
            Assert.Null(news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 4), news.PostDate);
            Assert.Equal("on-focus/04-01-new-structura", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new CustomsBgNewsSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(6, result.Count());
        }

        [Fact]
        public void GetOnFocusShouldReturnResults()
        {
            var provider = new CustomsBgOnFocusSource();
            var result = provider.GetLatestPublications();
            Assert.True(result.Any());
        }
    }
}
