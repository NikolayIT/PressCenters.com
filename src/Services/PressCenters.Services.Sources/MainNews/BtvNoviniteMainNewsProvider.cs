namespace PressCenters.Services.Sources.MainNews
{
    using AngleSharp;

    public class BtvNoviniteMainNewsProvider : BaseMainNewsProvider
    {
        public override RemoteMainNews GetMainNews()
        {
            var document = this.BrowsingContext.OpenAsync("http://btvnovinite.bg/").Result;

            var titleElement = document.QuerySelector(".main-articles-wrapper .text .title");
            var title = titleElement.TextContent.Trim();

            var url = titleElement.Attributes["href"].Value;
            url = "http://btvnovinite.bg" + url.Trim();

            var shortTitleElement = document.QuerySelector(".main-articles-wrapper .text .summary");
            var shortTitle = shortTitleElement?.TextContent?.Trim();

            var imageElement = document.QuerySelector(".main-articles-wrapper .leadingImage img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            var news = new RemoteMainNews
                           {
                               Title = title,
                               ShortTitle = shortTitle,
                               OriginalUrl = url,
                               ImageUrl = imageUrl,
                           };
            return news;
        }
    }
}
