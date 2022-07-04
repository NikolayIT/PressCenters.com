namespace PressCenters.Services.Sources.Tests.Ministries
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Ministries;

    using Xunit;

    public class EgovGovernmentBgSourceTests
    {
        [Theory]
        [InlineData("https://egov.government.bg/wps/portal/ministry-meu/press-center/news/BBKoalitia", "BBKoalitia")]
        [InlineData("https://egov.government.bg/wps/portal/ministry-meu/press-center/news/EI/", "EI")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new EgovGovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://egov.government.bg/wps/portal/ministry-meu/press-center/news/dogovormeuio";
            var provider = new EgovGovernmentBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("МЕУ възложи на \"Информационно обслужване\" изграждането на мобилно приложение за е-идентификация", news.Title);
            Assert.Contains("Министерство на електронното управление възложи на „Информационно обслужване“ АД", news.Content);
            Assert.Contains("За повече информация, вижте Договора по-долу.", news.Content);
            Assert.Contains("Договор за изграждане на мобилно приложение за електронна идентификация и електронно подписване", news.Content);
            Assert.DoesNotContain("Последна актуализация", news.Content);
            Assert.DoesNotContain(".jpg", news.Content);
            Assert.Equal("https://egov.government.bg/wps/wcm/connect/egov.government.bg-2818/baa42359-f696-4e9f-8f49-1aa44f99c983/MEU.jpg?MOD=AJPERES&CACHEID=ROOTWORKSPACE.Z18_PPGAHG800PLV6060GL92MR3OU3-baa42359-f696-4e9f-8f49-1aa44f99c983-o1RijCV", news.ImageUrl);
            Assert.Equal(new DateTime(2022, 4, 28), news.PostDate);
            Assert.Equal("dogovormeuio", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new EgovGovernmentBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
