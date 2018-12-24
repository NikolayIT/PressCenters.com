namespace PressCenters.Services.Sources.MainNews
{
    public class BtvNoviniteMainNewsProvider : BaseMainNewsProvider
    {
        private const string BaseUrl = "https://btvnovinite.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(BaseUrl);

            var titleElement = document.QuerySelector(".leading-articles .item .title");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector(".leading-articles .item .link");
            var url = BaseUrl + urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".leading-articles .item .image img");
            var imageUrl = "https:" + imageElement?.Attributes["src"]?.Value?.Trim();

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
