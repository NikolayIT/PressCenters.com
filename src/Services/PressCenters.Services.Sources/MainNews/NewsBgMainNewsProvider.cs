namespace PressCenters.Services.Sources.MainNews
{
    public class NewsBgMainNewsProvider : BaseMainNewsProvider
    {
        private const string BaseUrl = "https://news.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(BaseUrl);

            var titleElement = document.QuerySelector("#content-main .main-news a.main-thumb .news-info h2");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector("#content-main .main-news a.main-thumb");
            var url = urlElement.Attributes["href"].Value.Trim();

            var shortTitleElement = document.QuerySelector("#content-main .main-news a.main-thumb .news-info p");
            var shortTitle = shortTitleElement?.TextContent?.Trim();

            var imageElement = document.QuerySelector("#content-main .main-news a.main-thumb img.thumb");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            var news = new RemoteMainNews
            {
                Title = title,
                ShortTitle = shortTitle,
                OriginalUrl = url,
                ImageUrl = imageUrl,
            };
            return news;
        }
    }
}
