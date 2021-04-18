namespace PressCenters.Services.Sources.MainNews
{
    using System.Linq;

    using AngleSharp;

    using PressCenters.Common;

    public class ReutersMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.reuters.com";

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.GetElementsByTagName("span").FirstOrDefault(x => x.ClassName.StartsWith("MediaStoryCard__title___"));
            var title = titleElement?.TextContent.Trim();

            var url = this.BaseUrl + document.GetElementsByTagName("a").FirstOrDefault(x => x.ClassName.Contains("MediaStoryCard__basic_hero___"))?.Attributes["href"].Value.Trim();

            var imageUrl = document.ToHtml().GetStringBetween("\",\"url\":\"", "\"");
            if (!imageUrl.StartsWith("http"))
            {
                imageUrl = null;
            }

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
