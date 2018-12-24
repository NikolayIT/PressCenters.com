namespace PressCenters.Services.Sources.MainNews
{
    public class VestiBgMainNewsProvider : BaseMainNewsProvider
    {
        private const string BaseUrl = "https://www.vesti.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(BaseUrl);

            var titleElement = document.QuerySelector(".leading h2");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector(".leading a");
            var url = urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".leading img");
            var imageUrl = imageElement?.Attributes["data-original"]?.Value?.Trim();

            var news = new RemoteMainNews
            {
                Title = title,
                OriginalUrl = url,
                ImageUrl = imageUrl,
            };
            return news;
        }
    }
}
