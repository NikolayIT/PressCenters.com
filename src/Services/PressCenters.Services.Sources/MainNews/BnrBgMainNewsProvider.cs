namespace PressCenters.Services.Sources.MainNews
{
    using System;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;

    public class BnrBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl => "https://bnrnews.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            // The homepage hero is a looping nuka-carousel: every story is rendered three times -- a `prev-cloned`
            // copy, the real slide, then a `next-cloned` copy -- so the first /main/post/ anchor in the DOM is a
            // clone whose lazy-loaded background is sometimes still a `data:` placeholder (hence the missing image).
            // Anchor on the first real (non-clone) slide: that is the featured lead and the one whose picture is
            // actually loaded. Fall back to the first anchor's slide if the clone classes ever change.
            var slide = document.QuerySelector(".slide:not(.prev-cloned):not(.next-cloned)")
                        ?? document.QuerySelector("a[href*='/main/post/']")?.Closest(".slide");

            var link = slide?.QuerySelector("a[href*='/main/post/']");
            var title = link?.QuerySelector(".font-serif-bold")?.TextContent?.Trim();
            var url = this.MakeAbsoluteUrl(link?.GetAttribute("href"));

            var imageUrl = this.MakeAbsoluteUrl(ExtractSlideImageUrl(slide));

            return new RemoteMainNews(title, url, imageUrl);
        }

        // Each slide paints its photo as a CSS background-image on an inner element. Pick the first real picture,
        // skipping the lazy-load `data:` placeholder and the gradient-only overlay (which carries no url()).
        private static string ExtractSlideImageUrl(IElement slide)
        {
            if (slide == null)
            {
                return null;
            }

            foreach (var element in slide.QuerySelectorAll("[style*='background-image']"))
            {
                var match = Regex.Match(element.GetAttribute("style") ?? string.Empty, @"url\(([^)]+)\)");
                if (!match.Success)
                {
                    continue;
                }

                var imageUrl = match.Groups[1].Value.Trim('\'', '"');
                if (!string.IsNullOrWhiteSpace(imageUrl) && !imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    return imageUrl;
                }
            }

            return null;
        }
    }
}
