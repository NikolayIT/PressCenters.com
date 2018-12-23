namespace PressCenters.Worker.MainNewsProviders
{
    using System;

    using AngleSharp;

    public class NoviniBgMainNewsProvider : BaseMainNewsProvider
    {
        public override RemoteMainNews GetMainNews()
        {
            var document = this.BrowsingContext.OpenAsync("http://www.novini.bg/").Result;
            var titleElement = document.QuerySelector(".box_new_news .l_col .im .desc");
            var title = titleElement.TextContent.Trim();

            var urlElement = document.QuerySelector(".box_new_news .l_col a.im");
            var url = urlElement.Attributes["href"].Value.Trim();

            var imageElement = document.QuerySelector(".box_new_news .l_col .im img");
            var imageUrl = imageElement?.Attributes["src"]?.Value?.Trim();

            var news = new RemoteMainNews
            {
                Title = title,
                ShortTitle = null,
                OriginalUrl = url,
                ImageUrl = imageUrl,
            };
            Console.WriteLine(imageUrl);
            Console.WriteLine(url);
            Console.WriteLine(title);
            return news;
        }
    }
}
