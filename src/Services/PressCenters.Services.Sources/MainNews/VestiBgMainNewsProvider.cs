namespace PressCenters.Services.Sources.MainNews
{
    public class VestiBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.vesti.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".leading h2");
            var title = titleElement?.TextContent?.Trim();

            var urlElement = document.QuerySelector(".leading a");
            var url = this.MakeAbsoluteUrl(urlElement?.GetAttribute("href"));

            var imageElement = document.QuerySelector(".leading img");
            var imageUrl = this.MakeAbsoluteUrl(imageElement?.GetAttribute("data-original"));

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
