namespace PressCenters.Services.Sources.MainNews
{
    using System.Net;
    using System.Text;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Html.Parser;

    public abstract class BaseMainNewsProvider
    {
        public abstract RemoteMainNews GetMainNews();

        // TODO: Async
        public IDocument GetDocument(string url)
        {
            var configuration = Configuration.Default.WithDefaultLoader();
            var browsingContext = AngleSharp.BrowsingContext.New(configuration);
            return browsingContext.OpenAsync(url).GetAwaiter().GetResult();
        }

        // TODO: Async
        public IDocument GetDocument(string url, Encoding encoding)
        {
            var parser = new HtmlParser();
            var webClient = new WebClient { Encoding = encoding };
            var html = webClient.DownloadString(url);
            var document = parser.ParseDocument(html);
            return document;
        }
    }
}
