namespace PressCenters.Worker.MainNewsProviders
{
    using AngleSharp;

    public class VestiBgMainNewsProvider : BaseMainNewsProvider
    {
        public override RemoteMainNews GetMainNews()
        {
            var document = this.BrowsingContext.OpenAsync("http://www.vesti.bg/").Result;
            var titleElement = document.QuerySelector(".main-news .main-news-first h2 a span");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector(".main-news .main-news-first h2 a");
            var url = urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".main-news .main-news-first .img-link img");
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
