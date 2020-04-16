namespace PressCenters.Services.Sources.MainNews
{
    public class EuronewsMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.euronews.com";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl + "/?PageSpeed=noscript");

            var titleElement = document.QuerySelector(".c-first-topstory .media__main h1.media__body__title a")
                               ?? document.QuerySelector(".m-object__title__link--big-title");

            var title = titleElement.TextContent.Trim();

            var url = this.BaseUrl + titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".media__img__link img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
