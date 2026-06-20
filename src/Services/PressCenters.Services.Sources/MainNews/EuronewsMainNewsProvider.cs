namespace PressCenters.Services.Sources.MainNews
{
    public class EuronewsMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.euronews.com";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl + "/?PageSpeed=noscript");

            var linkElement = document.QuerySelector(".tc-hero a[href]");
            var title = linkElement?.GetAttribute("aria-label")?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                title = document.QuerySelector(".tc-hero__title")?.TextContent?.Trim();
            }

            var url = this.MakeAbsoluteUrl(linkElement?.GetAttribute("href"));

            var imageElement = document.QuerySelector(".tc-hero__poster img");
            var imageUrl = this.MakeAbsoluteUrl(imageElement?.GetAttribute("src"));

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
