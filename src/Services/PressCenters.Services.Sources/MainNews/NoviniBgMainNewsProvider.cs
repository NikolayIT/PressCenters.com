namespace PressCenters.Services.Sources.MainNews
{
    using System;
    using System.Linq;

    public class NoviniBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://novini.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            // novini.bg is a Next.js/Tailwind build with no semantic class names, so anchor on the stable
            // "/article/{id}" link pattern instead of volatile utility classes. The first such card is the lead.
            var linkElement = document.QuerySelector("a[href*='/article/']");
            var title = linkElement?.QuerySelector("h1, h2, h3")?.TextContent?.Trim();
            var url = this.MakeAbsoluteUrl(linkElement?.GetAttribute("href"));

            // The lead <img> is server-rendered without a src; the real image lives in <picture><source srcset>.
            var srcset = linkElement?.QuerySelector("source[srcset]")?.GetAttribute("srcset");
            var firstImage = srcset?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            var imageUrl = this.MakeAbsoluteUrl(firstImage);

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
