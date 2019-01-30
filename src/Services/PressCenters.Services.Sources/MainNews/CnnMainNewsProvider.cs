namespace PressCenters.Services.Sources.MainNews
{
    using System.Net.Http;

    using AngleSharp.Parser.Html;

    using Newtonsoft.Json;

    public class CnnMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://edition.cnn.com";

        public override RemoteMainNews GetMainNews()
        {
            var httpClient = new HttpClient();
            var response = httpClient
                .GetAsync(
                    "https://edition.cnn.com/data/ocs/section/index.html:intl_homepage1-zone-1/views/zones/common/zone-manager.izl")
                .GetAwaiter().GetResult();
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var dataObject = JsonConvert.DeserializeObject<JsonDataObject>(content);

            var parser = new HtmlParser();
            var document = parser.Parse(dataObject.Html);

            var titleElement = document.QuerySelector("#intl_homepage1-zone-1 .cd--article h2");
            var subTitleElement = document.QuerySelector("#intl_homepage1-zone-1 .cd--article .cd__headline-text strong");
            var title = $"{titleElement?.TextContent?.Trim()} ({subTitleElement?.TextContent?.Trim()})";

            var urlElement = document.QuerySelector("#intl_homepage1-zone-1 .cd--article a");
            var url = this.BaseUrl + urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector("#intl_homepage1-zone-1 .cd--article .cd__wrapper .media noscript img");
            var imageUrl = "https:" + imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }

        public class JsonDataObject
        {
            public string Html { get; set; }
        }
    }
}
