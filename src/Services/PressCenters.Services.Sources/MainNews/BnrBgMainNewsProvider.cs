namespace PressCenters.Services.Sources.MainNews
{
    using System.Text.RegularExpressions;

    public class BnrBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl => "https://bnrnews.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            // The BNR news portal opens with a full-width hero banner -- the first /main/post/ link. Its headline
            // sits in a .font-serif-bold element; its picture is a CSS background-image on an ancestor element.
            var hero = document.QuerySelector("a[href*='/main/post/']");

            var title = hero?.QuerySelector(".font-serif-bold")?.TextContent?.Trim();
            var url = this.MakeAbsoluteUrl(hero?.GetAttribute("href"));

            var bannerStyle = hero?.Closest("[style*='background-image']")?.GetAttribute("style");
            var imageUrl = this.MakeAbsoluteUrl(ExtractBackgroundImageUrl(bannerStyle));

            return new RemoteMainNews(title, url, imageUrl);
        }

        private static string ExtractBackgroundImageUrl(string style)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                return null;
            }

            var match = Regex.Match(style, @"url\(([^)]+)\)");
            return match.Success ? match.Groups[1].Value.Trim('\'', '"') : null;
        }
    }
}
