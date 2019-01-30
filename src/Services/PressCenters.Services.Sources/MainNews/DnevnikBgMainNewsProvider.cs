namespace PressCenters.Services.Sources.MainNews
{
    public class DnevnikBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.dnevnik.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".primary-article-v1 h1");
            var title = titleElement?.TextContent?.Trim();

            var urlElement = document.QuerySelector(".primary-article-v1 h1 a");
            var url = this.BaseUrl + urlElement?.Attributes["href"]?.Value?.Trim();

            var imageElement = document.QuerySelector(".primary-article-v1 a img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
