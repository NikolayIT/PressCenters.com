namespace PressCenters.Services.Sources.MainNews
{
    public class ReutersMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.reuters.com";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector("#topStory .story-title a");
            var title = titleElement.TextContent.Trim();

            var url = this.BaseUrl + titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector("#topStory .story-photo img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();
            if (imageUrl != null && !imageUrl.StartsWith("http"))
            {
                imageUrl = "https:" + imageUrl;
            }

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
