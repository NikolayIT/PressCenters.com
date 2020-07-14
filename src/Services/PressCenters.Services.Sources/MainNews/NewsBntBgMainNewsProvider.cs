namespace PressCenters.Services.Sources.MainNews
{
    public class NewsBntBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://bntnews.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl + "/bg");

            var titleElement = document.QuerySelector(".top-img h1.home-news-title");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector(".top-img .img-wrap a");
            var url = urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".top-img .img-wrap img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
