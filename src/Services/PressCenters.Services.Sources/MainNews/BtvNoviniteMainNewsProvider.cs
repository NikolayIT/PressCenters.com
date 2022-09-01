namespace PressCenters.Services.Sources.MainNews
{
    public class BtvNoviniteMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://btvnovinite.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".news-article h3");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector(".news-article a");
            var url = this.BaseUrl + urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".news-article img");
            var imageUrl = imageElement?.Attributes["data-src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
