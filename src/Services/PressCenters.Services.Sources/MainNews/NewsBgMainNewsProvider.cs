namespace PressCenters.Services.Sources.MainNews
{
    public class NewsBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://news.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector("#content-main .main-news a.main-thumb .news-info h2");
            var title = titleElement.TextContent.Trim(); // $"{title} ({shortTitle})"

            var urlElement = document.QuerySelector("#content-main .main-news a.main-thumb");
            var url = urlElement.Attributes["href"].Value.Trim();

            //// var shortTitleElement = document.QuerySelector("#content-main .main-news a.main-thumb .news-info p");
            //// var shortTitle = shortTitleElement?.TextContent?.Trim();
            var imageElement = document.QuerySelector("#content-main .main-news a.main-thumb img.thumb");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
