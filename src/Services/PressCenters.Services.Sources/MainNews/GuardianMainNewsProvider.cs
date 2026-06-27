namespace PressCenters.Services.Sources.MainNews
{
    using System.Text.RegularExpressions;

    public class GuardianMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl => "https://www.theguardian.com/international";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            // The international front page marks its lead story's link with data-link-name "... | card-@1" and
            // exposes the headline as that link's aria-label. The lead image is the first Guardian-CDN image.
            var leadLink = document.QuerySelector("a[data-link-name*='card-@1'][aria-label]")
                           ?? document.QuerySelector("a[aria-label][href*='/20']");

            var title = leadLink?.GetAttribute("aria-label")?.Trim();
            var url = this.MakeAbsoluteUrl(leadLink?.GetAttribute("href"));

            var imageElement = document.QuerySelector("img[src*='i.guim.co.uk']");
            var imageUrl = imageElement?.GetAttribute("src");

            // Front-page thumbnails are served at width=98; the unsigned (s=none) image server lets us request a
            // larger render, which the downstream image resizer needs to produce a usable tile.
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.Contains("s=none"))
            {
                imageUrl = Regex.Replace(imageUrl, "width=\\d+", "width=620");
            }

            return new RemoteMainNews(title, url, this.MakeAbsoluteUrl(imageUrl));
        }
    }
}
