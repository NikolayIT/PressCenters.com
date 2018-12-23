namespace PressCenters.Worker.MainNewsProviders
{
    using AngleSharp;

    public class NovaBgMainNewsProvider : BaseMainNewsProvider
    {
        public override RemoteMainNews GetMainNews()
        {
            var document = this.BrowsingContext.OpenAsync("https://nova.bg/").Result;

            var titleElement = document.QuerySelector(".main-accent-wrapper .thumb-title h1 a");
            var title = titleElement.TextContent.Trim();

            var url = titleElement.Attributes["href"].Value;

            var imageElement = document.QuerySelector(".main-accent-wrapper .img-cont img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            var news = new RemoteMainNews
            {
                Title = title,
                ShortTitle = null,
                OriginalUrl = url,
                ImageUrl = imageUrl,
            };

            return news;
        }
    }
}
