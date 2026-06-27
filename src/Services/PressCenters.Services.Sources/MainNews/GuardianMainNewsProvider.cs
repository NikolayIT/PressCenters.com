namespace PressCenters.Services.Sources.MainNews
{
    using System.Text.RegularExpressions;

    public class GuardianMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl => "https://www.theguardian.com/international";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            // The page opens with an opinion/features strip, so the real news lead is the first card tagged both
            // "card-@1" (lead of its container) and "media-picture" (a news card with a photo). The headline is
            // exposed as the link's aria-label and the photo lives in the same card wrapper div.
            var leadLink = document.QuerySelector("a[data-link-name*='card-@1'][data-link-name*='media-picture'][aria-label]")
                           ?? document.QuerySelector("a[data-link-name*='media-picture'][aria-label]");

            var title = leadLink?.GetAttribute("aria-label")?.Trim();
            var url = this.MakeAbsoluteUrl(leadLink?.GetAttribute("href"));

            var imageElement = leadLink?.Closest("div")?.QuerySelector("img[src*='i.guim.co.uk']");
            var imageUrl = imageElement?.GetAttribute("src");

            // Front-page renders come at a small width; the unsigned (s=none) image server lets us request a
            // larger one, which the downstream image resizer needs to produce a usable tile.
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.Contains("s=none"))
            {
                imageUrl = Regex.Replace(imageUrl, "width=\\d+", "width=620");
            }

            return new RemoteMainNews(title, url, this.MakeAbsoluteUrl(imageUrl));
        }
    }
}
