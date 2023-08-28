namespace PressCenters.Services.Sources.MainNews
{
    public class BtaBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.bta.bg/bg";

        public override bool UseProxy => true;

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".leading-section .news-card__title a");
            var title = titleElement.TextContent.Trim().Trim('.').Trim() + "...";

            var url = this.BaseUrl + titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".leading-section .news-card__image img");
            var imageUrl = this.BaseUrl + imageElement?.Attributes["data-src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
