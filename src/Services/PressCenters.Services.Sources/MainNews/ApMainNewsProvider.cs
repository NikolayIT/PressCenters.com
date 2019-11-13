namespace PressCenters.Services.Sources.MainNews
{
    public class ApMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.apnews.com";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector("div[data-key='main-story'] a[data-key='card-headline']");
            var title = titleElement.TextContent.Trim();

            var url = this.BaseUrl + titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector("div[data-key='main-story'] div[data-key='media-placeholder'] img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim()?.Replace("/800.jpeg", "/400.jpeg");

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
