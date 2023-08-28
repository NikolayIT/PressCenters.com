namespace PressCenters.Services.Sources.MainNews
{
    public class ApMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.apnews.com";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".PageListStandardE-leadPromo-info a");
            var title = titleElement.TextContent.Trim();

            var url = titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".PageListStandardE-leadPromo-media img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
