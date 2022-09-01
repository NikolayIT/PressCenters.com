namespace PressCenters.Services.Sources.MainNews
{
    public class MediapoolBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.mediapool.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".c-article-item_accent .c-article-item__content a");
            var title = titleElement.TextContent.Trim().Trim('.').Trim();

            var url = titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".c-article-item_accent img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
