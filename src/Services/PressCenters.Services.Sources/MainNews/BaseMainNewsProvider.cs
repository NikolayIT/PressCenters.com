namespace PressCenters.Services.Sources.MainNews
{
    using System.Net;
    using System.Text;

    using AngleSharp.Dom;
    using AngleSharp.Parser.Html;

    using PressCenters.Common;

    public abstract class BaseMainNewsProvider
    {
        public abstract string BaseUrl { get; }

        public abstract RemoteMainNews GetMainNews();

        public IDocument GetDocument(string url, Encoding encoding = null)
        {
            var parser = new HtmlParser();
            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.UserAgent, GlobalConstants.DefaultUserAgent);
            if (encoding != null)
            {
                webClient.Encoding = encoding;
            }

            var html = webClient.DownloadString(url);
            var document = parser.Parse(html);
            return document;
        }
    }
}
