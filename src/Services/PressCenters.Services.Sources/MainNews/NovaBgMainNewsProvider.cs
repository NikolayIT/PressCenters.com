namespace PressCenters.Services.Sources.MainNews
{
    public class NovaBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://nova.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".main-accent-wrapper .thumb-title h1 a");
            var title = titleElement.TextContent.Trim();

            var url = titleElement.Attributes["href"].Value;

            var imageElement = document.QuerySelector(".main-accent-wrapper .img-cont img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
