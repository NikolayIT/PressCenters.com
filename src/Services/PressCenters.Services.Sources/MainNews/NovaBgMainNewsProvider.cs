namespace PressCenters.Services.Sources.MainNews
{
    public class NovaBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://nova.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".main-accent-wrapper .thumb-title h1 a");
            var title = titleElement?.TextContent?.Trim();

            var url = this.MakeAbsoluteUrl(titleElement?.GetAttribute("href"));

            var imageElement = document.QuerySelector(".main-accent-wrapper .img-cont img");
            var imageUrl = this.MakeAbsoluteUrl(imageElement?.GetAttribute("src"));

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
