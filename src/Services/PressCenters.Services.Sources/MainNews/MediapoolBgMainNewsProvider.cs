namespace PressCenters.Services.Sources.MainNews
{
    public class MediapoolBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.mediapool.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".hot_news h3.big_title a");
            var title = titleElement.TextContent.Trim().Trim('.').Trim();

            var url = titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".hot_news .news_pic img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
