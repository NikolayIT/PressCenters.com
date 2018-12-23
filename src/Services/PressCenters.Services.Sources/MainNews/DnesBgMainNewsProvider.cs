namespace PressCenters.Services.Sources.MainNews
{
    using System.Text;

    public class DnesBgMainNewsProvider : BaseMainNewsProvider
    {
        private const string BaseUrl = "https://www.dnes.bg";

        public override RemoteMainNews GetMainNews()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var document = this.GetDocument(BaseUrl, Encoding.GetEncoding("windows-1251"));

            var titleElement = document.QuerySelector(".top-news-wrapper .left .top-news .image-title > a");
            var title = titleElement.TextContent.Trim();
            var url = BaseUrl + titleElement.Attributes["href"].Value.Trim();

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
