namespace PressCenters.Services.Sources.MainNews
{
    using AngleSharp;

    public class DnesBgMainNewsProvider : BaseMainNewsProvider
    {
        public override RemoteMainNews GetMainNews()
        {
            var document = this.BrowsingContext.OpenAsync("http://www.dnes.bg/").Result;
            var titleElement = document.QuerySelector(".top-news-wrapper .left .top-news .image-title > a");
            var title = titleElement.TextContent.Trim();
            var url = "http://www.dnes.bg" + titleElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".top-news-wrapper .left .top-news .first a img");
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
