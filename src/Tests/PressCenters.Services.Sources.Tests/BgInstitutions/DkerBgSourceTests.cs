namespace PressCenters.Services.Sources.Tests.BgInstitutions
{
    using System;
    using System.Linq;

    using PressCenters.Services.Sources.BgInstitutions;

    using Xunit;

    public class DkerBgSourceTests
    {
        [Theory]
        [InlineData("https://www.dker.bg/news/839/65/pokana-za-obschestveno-obszhdane-proekt-na-pravila-za-izmenenie-i-doplnenie-na-pravilata-za-trgoviya-s-elektricheska-energiya.html", "pokana-za-obschestveno-obszhdane-proekt-na-pravila-za-izmenenie-i-doplnenie-na-pravilata-za-trgoviya-s-elektricheska-energiya")]
        [InlineData("https://www.dker.bg/news/842/65/vodosnabdyavane-i-kanalizatsiya-vidin-eood-sche-investira-4-5-mln-lv-za-da-garantira-nadezhdnoto-snabdyavane-s-kachestvena-pitejna-voda.html", "vodosnabdyavane-i-kanalizatsiya-vidin-eood-sche-investira-4-5-mln-lv-za-da-garantira-nadezhdnoto-snabdyavane-s-kachestvena-pitejna-voda")]
        public void ExtractIdFromUrlShouldWorkCorrectly(string url, string id)
        {
            var provider = new DkerBgSource();
            var result = provider.ExtractIdFromUrl(url);
            Assert.Equal(id, result);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectly()
        {
            const string NewsUrl = "https://www.dker.bg/news/841/65/predlaganite-ot-kevr-promeni-v-pravilata-za-trgoviya-s-elektricheska-energiya-prodlzhavat-reformata-na-pazara-na-balansiraschi-uslugi.html";
            var provider = new DkerBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("ПРЕДЛАГАНИТЕ ОТ КЕВР ПРОМЕНИ В ПРАВИЛАТА ЗА ТЪРГОВИЯ С ЕЛЕКТРИЧЕСКА ЕНЕРГИЯ ПРОДЪЛЖАВАТ РЕФОРМАТА НА ПАЗАРА НА БАЛАНСИРАЩИ УСЛУГИ", news.Title);
            Assert.Equal("predlaganite-ot-kevr-promeni-v-pravilata-za-trgoviya-s-elektricheska-energiya-prodlzhavat-reformata-na-pazara-na-balansiraschi-uslugi", news.RemoteId);
            Assert.Equal(new DateTime(2022, 8, 3), news.PostDate);
            Assert.Contains("Комисията за енергийно и водно регулиране проведе обществени обсъждания на проекти на правила за", news.Content);
            Assert.Contains("след което двата нормативни акта ще бъдат изпратени за обнародване в „Държавен вестник“.", news.Content);
            Assert.DoesNotContain("03.08.2022", news.Content);
            Assert.DoesNotContain("ПРЕДЛАГАНИТЕ ОТ КЕВР ПРОМЕНИ В ПРАВИЛАТА", news.Content);
            Assert.Equal("https://www.dker.bg/uploads/news/id841/20220803_110047.jpg", news.ImageUrl);
        }

        [Fact]
        public void ParseRemoteNewsShouldWorkCorrectlyWithoutImage()
        {
            const string NewsUrl = "https://www.dker.bg/news/849/65/pokana-za-obschestveno-obszhdane-na-proekt-na-reshenie-za-odobryavane-na-biznes-plana-vik-zlatni-pyastsi-ood-i-za-utvrzhdavane-i-odobryavane-tseni-na-vik-uslugi-na-vik-zlatni-pyastsi-ood.html";
            var provider = new DkerBgSource();
            var news = provider.GetPublication(NewsUrl);
            Assert.Equal(NewsUrl, news.OriginalUrl);
            Assert.Equal("Покана за обществено обсъждане на проект на решение за одобряване на бизнес плана „ВиК – Златни пясъци“ ООД и за утвърждаване и одобряване цени на ВиК услуги на „ВиК – Златни пясъци“ ООД", news.Title);
            Assert.Equal("pokana-za-obschestveno-obszhdane-na-proekt-na-reshenie-za-odobryavane-na-biznes-plana-vik-zlatni-pyastsi-ood-i-za-utvrzhdavane-i-odobryavane-tseni-na-vik-uslugi-na-vik-zlatni-pyastsi-ood", news.RemoteId);
            Assert.Equal(new DateTime(2022, 8, 31), news.PostDate);
            Assert.Contains("На 07.09.2022 г. от 10.10 ч. в сградата на КЕВР на бул. „Княз Дондуков“ 8-10", news.Content);
            Assert.Contains("ВиК услуги на „ВиК – Златни пясъци“ ООД е публикуван на интернет страницата на КЕВР", news.Content);
            Assert.DoesNotContain("Покана за обществено обсъждане на проект на решение", news.Content);
            Assert.Null(news.ImageUrl);
        }

        [Fact]
        public void GetNewsShouldReturnResults()
        {
            var provider = new DkerBgSource();
            var result = provider.GetLatestPublications();
            Assert.Equal(5, result.Count());
        }
    }
}
