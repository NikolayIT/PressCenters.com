namespace PressCenters.Services.Sources.MainNews
{
    public class BnrBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl => "https://bnr.bg";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            // The homepage news grid leads with a single oversized card (Tailwind's "first-of-type:col-span-2"
            // rule). Its headline sits in a .font-serif-bold span and the card links out to the bnrnews.bg
            // article. Matching on the class substring avoids escaping Tailwind's colon-laden class names.
            var leadCard = document.QuerySelector("a[class*='col-span-2'][href*='/post/']");

            var title = leadCard?.QuerySelector(".font-serif-bold")?.TextContent?.Trim();
            var url = this.MakeAbsoluteUrl(leadCard?.GetAttribute("href"));

            var imageElement = leadCard?.QuerySelector("img");
            var imageUrl = this.MakeAbsoluteUrl(imageElement?.GetAttribute("src"));

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
