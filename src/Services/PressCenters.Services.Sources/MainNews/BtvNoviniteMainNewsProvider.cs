namespace PressCenters.Services.Sources.MainNews
{
    using System;
    using System.Linq;

    public class BtvNoviniteMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://btvnovinite.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            // The homepage lead story is the single ".big-news" block in the top "leading" section.
            var titleElement = document.QuerySelector(".big-news .small-title");
            var title = titleElement?.TextContent?.Trim();

            var url = this.MakeAbsoluteUrl(titleElement?.GetAttribute("href"));

            // Images are lazy-loaded: the <img src> is a 1x1 transparent placeholder and the real
            // image URLs live in "data-srcset" (a comma-separated srcset whose first entry is the
            // largest variant). Take that first URL rather than the worthless placeholder.
            var imageElement = document.QuerySelector(".big-news img");
            var imageUrl = this.MakeAbsoluteUrl(ExtractFirstSrcSetUrl(imageElement?.GetAttribute("data-srcset")));

            return new RemoteMainNews(title, url, imageUrl);
        }

        private static string ExtractFirstSrcSetUrl(string srcSet)
        {
            return srcSet?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()?
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
        }
    }
}
