namespace PressCenters.Services.Sources.Tests.Municipalities
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.Municipalities;

    using Xunit;

    public class SofiaBgSourceTests
    {
        [Theory]
        [InlineData("https://www.sofia.bg/web/guest/news-archive-2016/-/asset_publisher/hultJKe9uK9Q/content/prez-mesec-uni-se-organizirat-cetiri-mobilni-punkta-za-priemane-na-opasni-otpad-ci?inheritRedirect=false&redirect=https%3A%2F%2Fwww.sofia.bg%3A443%2Fweb%2Fguest%2Fnews-archive-2016%3Fp_p_id%3D101_INSTANCE_hultJKe9uK9Q%26p_p_lifecycle%3D0%26p_p_state%3Dnormal%26p_p_mode%3Dview%26p_p_col_id%3Dcolumn-1%26p_p_col_count%3D1%26_101_INSTANCE_hultJKe9uK9Q_advancedSearch%3Dfalse%26_101_INSTANCE_hultJKe9uK9Q_keywords%3D%26_101_INSTANCE_hultJKe9uK9Q_delta%3D10%26p_r_p_564233524_resetCur%3Dfalse%26_101_INSTANCE_hultJKe9uK9Q_cur%3D19%26_101_INSTANCE_hultJKe9uK9Q_andOperator%3Dtrue", "prez-mesec-uni-se-organizirat-cetiri-mobilni-punkta-za-priemane-na-opasni-otpad-ci")]
        [InlineData("https://www.sofia.bg/w/%D0%9D%D0%B0-14-%D1%8F%D0%BD%D1%83%D0%B0%D1%80%D0%B8-%D0%BE%D1%82%D0%B1%D0%B5%D0%BB%D1%8F%D0%B7%D0%B2%D0%B0%D0%BC%D0%B5-122-%D0%B3%D0%BE%D0%B4%D0%B8%D0%BD%D0%B8-%D0%B3%D1%80%D0%B0%D0%B4%D1%81%D0%BA%D0%B8-%D1%82%D1%80%D0%B0%D0%BD%D1%81%D0%BF%D0%BE%D1%80", "На-14-януари-отбелязваме-122-години-градски-транспор")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new SofiaBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.sofia.bg/w/%D0%9D%D0%B0-14-%D1%8F%D0%BD%D1%83%D0%B0%D1%80%D0%B8-%D0%BE%D1%82%D0%B1%D0%B5%D0%BB%D1%8F%D0%B7%D0%B2%D0%B0%D0%BC%D0%B5-122-%D0%B3%D0%BE%D0%B4%D0%B8%D0%BD%D0%B8-%D0%B3%D1%80%D0%B0%D0%B4%D1%81%D0%BA%D0%B8-%D1%82%D1%80%D0%B0%D0%BD%D1%81%D0%BF%D0%BE%D1%80";
            var provider = new SofiaBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("На 14 януари отбелязваме 122 години градски транспорт в София", news.Title);
            Assert.Equal("На-14-януари-отбелязваме-122-години-градски-транспор", news.RemoteId);

            // The article no longer publishes a date paragraph, so the parser falls back to "now".
            Assert.Equal(DateTime.Now.Date, news.PostDate.Date);
            Assert.Contains("На 14 януари 1901 г. в София тръгва първият електрически трамвай, което поставя началото на Столичния градски транспорт.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.StartsWith("https://www.sofia.bg/documents/d/guest/2022-01-12-122-godini-gradski-transport", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsWithoutDateTimeShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.sofia.bg/w/%D0%97%D0%B0%D0%BF%D0%BE%D1%87%D0%BD%D0%B0-%D1%81%D1%82%D1%80%D0%BE%D0%B8%D1%82%D0%B5%D0%BB%D1%81%D1%82%D0%B2%D0%BE%D1%82%D0%BE-%D0%BD%D0%B0-%D0%BD%D0%BE%D0%B2-%D0%BA%D0%BE%D1%80%D0%BF%D1%83%D1%81-%D0%BD%D0%B0-30-%D0%94%D0%93-%E2%80%9E%D0%A0%D0%B0%D0%B4";
            var provider = new SofiaBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Започна строителството на нов корпус на ДГ № 30 „Радецки“ в София", news.Title);
            Assert.Equal("Започна-строителството-на-нов-корпус-на-30-ДГ-„Рад", news.RemoteId);
            Assert.Equal(DateTime.Now.Date, news.PostDate.Date);
            Assert.Contains("Днес започна изграждането на нов корпус на ДГ № 30 „Радецки“ в София", news.Content);
            Assert.Contains("както и кметът на район „Изгрев“ д-р Делян Георгиев.", news.Content);
            Assert.DoesNotContain(news.Title, news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetLatestPublicationsShouldReturnResults()
        {
            var provider = new SofiaBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
