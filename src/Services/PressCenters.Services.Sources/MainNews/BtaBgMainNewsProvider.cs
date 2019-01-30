namespace PressCenters.Services.Sources.MainNews
{
    public class BtaBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "http://www.bta.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".top-stories .bc h4 a");
            var title = titleElement.TextContent.Trim().Trim('.').Trim();

            var url = this.BaseUrl + titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".top-stories .bc li img");
            var imageUrl = this.BaseUrl + imageElement?.Attributes["src"]?.Value?.Trim();

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
