namespace PressCenters.Services.Sources.MainNews
{
    public class NewsBntBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://bntnews.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".top-news .big-title a");
            var title = titleElement?.TextContent?.Trim();
            var url = this.MakeAbsoluteUrl(titleElement?.GetAttribute("href"));

            var imageElement = document.QuerySelector(".top-news .img-wrap img");
            var imageUrl = this.MakeAbsoluteUrl(imageElement?.GetAttribute("src"));

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
