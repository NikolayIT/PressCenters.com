namespace PressCenters.Services.Sources.MainNews
{
    public class NewsBntBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://news.bnt.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl + "/bg");

            var titleElement = document.QuerySelector(".teaser-box h2");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector(".teaser-box a");
            var url = this.BaseUrl + urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".teaser-box a img");
            var imageUrl = imageElement?.Attributes["data-original"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
