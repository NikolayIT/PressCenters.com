namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources;
    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class GovernmentBgSourceTests
    {
        [Theory]
        [InlineData("/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&n=3737&g=", "3737")]
        [InlineData("/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&n=3734", "3734")]
        [InlineData("http://www.government.bg/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&n=3737&g=", "3737")]
        [InlineData("http://www.government.bg/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&n=3734", "3734")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new GovernmentBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.government.bg/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&n=3737&g=";
            var provider = new GovernmentBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Представители на правителството се срещнаха с десетте смесени търговски камари", news.Title);
            Assert.Equal("3737", news.RemoteId);
            Assert.Null(news.ShortContent);
            Assert.Equal(new DateTime(2016, 1, 27).Date, news.PostDate.Date);
            Assert.Contains("„Не можем да очакваме нови инвестиции без подобряване на съдебната система.", news.Content);
            Assert.Contains("Духът излезе от бутилката. Имаме достатъчно енергия да доведем процесите докрай”.", news.Content);
            Assert.True(!news.Content.Contains("/fce/001/0212/images/27012016.JPG"));
            Assert.Equal("http://www.government.bg/fce/001/0212/images/27012016.JPG", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWhenNoImageIsAvailable()
        {
            const string NewsUrl = "http://www.government.bg/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&n=3736&g=";
            var provider = new GovernmentBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Бойко Борисов: Мафията има интерес да няма правителство", news.Title);
            Assert.Null(news.ShortContent);
            Assert.Contains("Мафията има интерес да няма правителство,", news.Content);
            Assert.Contains("Повече по темата може да прочетете от стенограмата, публикувана тук.", news.Content);
            Assert.Contains("http://www.government.bg/fce/001/0212/files/2701_stenograma.doc", news.Content);
            Assert.Equal("http://www.government.bg/fce/001/tmpl/bigimg/gerb.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2016, 1, 27).Date, news.PostDate.Date);
            Assert.Equal("3736", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWhenMoreThanOneImageIsAvailable()
        {
            const string NewsUrl = "http://www.government.bg/cgi-bin/e-cms/vis/vis.pl?s=001&p=0212&n=3706&g=";
            var provider = new GovernmentBgSource();
            var news = provider.ParseRemoteNews(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Премиерът Борисов поздрави първите бенефициенти по новата ПРСР", news.Title);
            Assert.Null(news.ShortContent);
            Assert.Contains("Министър-председателят Бойко Борисов, министърът на земеделието", news.Content);
            Assert.Contains("първите договори са в размер на над 5 млн.", news.Content);
            Assert.Contains("http://www.government.bg/fce/001/0212/images/4052.jpg", news.Content);
            Assert.True(!news.Content.Contains("images/3931.jpg"));
            Assert.Equal("http://www.government.bg/fce/001/0212/images/3931.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2015, 12, 9).Date, news.PostDate.Date);
            Assert.Equal("3706", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new GovernmentBgSource();
            var result = provider.GetLatestPublications(new LocalPublicationsInfo { LastLocalId = string.Empty });
            Assert.True(result.News.Count() >= 25);
            Assert.True(int.Parse(result.LastNewsIdentifier) > 3736);
        }
    }
}
