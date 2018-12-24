namespace PressCenters.Services.Sources.MainNews
{
    public class NoviniBgMainNewsProvider : BaseMainNewsProvider
    {
        private const string BaseUrl = "https://novini.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(BaseUrl);

            var titleElement = document.QuerySelector(".content-left .leading-news .first h3");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector(".content-left .leading-news .first a");
            var url = urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".content-left .leading-news .first img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            var news = new RemoteMainNews
            {
                Title = title,
                ShortTitle = null,
                OriginalUrl = url,
                ImageUrl = imageUrl,
            };
            return news;
        }
    }
}
