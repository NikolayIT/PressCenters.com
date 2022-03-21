namespace PressCenters.Services.Sources.MainNews
{
    using System;
    using System.Net.Http;

    using AngleSharp.Dom;
    using AngleSharp.Html.Parser;

    using PressCenters.Common;

    public abstract class BaseMainNewsProvider
    {
        public abstract string BaseUrl { get; }

        public abstract RemoteMainNews GetMainNews();

        public IDocument GetDocument(string url)
        {
            var parser = new HtmlParser();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", GlobalConstants.DefaultUserAgent);
            //// httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            //// httpClient.DefaultRequestHeaders.Add("accept-language", "bg,en-US;q=0.9,en;q=0.8");
            var request = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url) { Version = new Version(2, 0) }).GetAwaiter().GetResult();
            var html = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var document = parser.ParseDocument(html);
            return document;
        }
    }
}
