namespace PressCenters.Services.Sources.MainNews
{
    public class CnnMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl => "https://edition.cnn.com";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            // The homepage leads with a "lead-plus-headlines" zone whose first headline is the top story.
            // CNN ships both a text-only and an "-with-images" variant of that zone; take whichever comes first.
            // Inside the lead card the image and the headline live in separate anchors, so anchor everything to
            // the zone: the first headline-text is the title and the first dam image is the lead picture.
            var zone = document.QuerySelector(".container_lead-plus-headlines")
                       ?? document.QuerySelector(".container_lead-plus-headlines-with-images");

            var headlineElement = zone?.QuerySelector(".container__headline-text");
            var title = headlineElement?.TextContent?.Trim().Trim('.').Trim();
            var url = this.MakeAbsoluteUrl(headlineElement?.Closest("a")?.GetAttribute("href"));

            var imageElement = zone?.QuerySelector("img.image__dam-img");
            var imageUrl = this.MakeAbsoluteUrl(imageElement?.GetAttribute("src"));

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
