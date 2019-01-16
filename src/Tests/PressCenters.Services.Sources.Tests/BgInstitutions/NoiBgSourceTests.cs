namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class NoiBgSourceTests
    {
        [Theory]
        [InlineData("http://www.noi.bg/newsbg/5373-statscontactcenter", "5373")]
        [InlineData("http://www.noi.bg/newsbg/2011/1-2011-11-27-17-38-26", "1")]
        public void ExtractIdFromPressUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new NoiBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.noi.bg/newsbg/5364-dohoddecember2018";
            var provider = new NoiBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Средният осигурителен доход за страната за ноември 2018 г. е 917,00 лева", news.Title);
            Assert.Contains("Националният осигурителен институт обявява, че размерът на средния осигурителен доход", news.Content);
            Assert.Contains("месец декември 2018 г., съгласно чл. 70, ал. 2 от Кодекса за социално осигуряване.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.DoesNotContain("10 Януари 2019", news.Content);
            Assert.DoesNotContain("<img", news.Content);
            Assert.DoesNotContain("noi_12.jpg", news.Content);
            Assert.DoesNotContain("Предишна", news.Content);
            Assert.Equal("http://www.noi.bg/images/bg/News/images/noi_12.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2019, 1, 10, 10, 2, 0), news.PostDate);
            Assert.Equal("5364", news.RemoteId);
        }

        [Fact]
        public void ParseRemoteNewsWithoutImageShouldWorkCorrectly()
        {
            const string NewsUrl = "http://www.noi.bg/newsbg/2011/2-2011-11-27-17-38-37";
            var provider = new NoiBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Заявление за потвърждаване на осигурителни периоди, придобити по законодателството на бившата ГДР.", news.Title);
            Assert.Contains("Заявление за потвърждаване на осигурителни периоди, придобити по законодателството на бившата ГДР.", news.Content);
            Assert.Contains("http://www.noi.bg/images/bg/users/forms/ermd/Zaqvl_potv_na_osig_periodi_GDR.doc", news.Content);
            Assert.Contains("/images/icons/doc.gif", news.Content);
            Assert.DoesNotContain("27 Ноември 2011", news.Content);
            Assert.DoesNotContain("Предишна", news.Content);
            Assert.DoesNotContain("Посещения", news.Content);
            Assert.Equal("/images/sources/noi.bg.jpg", news.ImageUrl);
            Assert.Equal(new DateTime(2011, 11, 27, 19, 18, 0), news.PostDate);
            Assert.Equal("2", news.RemoteId);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new NoiBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
