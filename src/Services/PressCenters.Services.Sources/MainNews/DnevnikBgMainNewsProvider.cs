namespace PressCenters.Services.Sources.MainNews
{
    public class DnevnikBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.dnevnik.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".main-column .column-content .card-title-link");
            var title = titleElement?.TextContent?.Trim();
            var url = this.MakeAbsoluteUrl(titleElement?.GetAttribute("href"));

            var imageElement = document.QuerySelector(".main-column .column-content .card-figure img");
            var imageUrl = this.MakeAbsoluteUrl(imageElement?.GetAttribute("src"));

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
