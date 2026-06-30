namespace PressCenters.Services.Sources.MainNews
{
    using System;
    using System.Net;
    using System.Net.Http;

    using AngleSharp.Dom;
    using AngleSharp.Html.Parser;

    using PressCenters.Common;

    public abstract class BaseMainNewsProvider
    {
        public abstract string BaseUrl { get; }

        public virtual bool UseProxy => false;

        public abstract RemoteMainNews GetMainNews();

        public IDocument GetDocument(string url)
        {
            return new HtmlParser().ParseDocument(this.GetContent(url));
        }

        // Fetches the raw response body for url using the same transport rules as GetDocument (optional proxy
        // wrap, gzip/brotli decompression, browser User-Agent, HTTP/2). Providers whose source is not HTML --
        // e.g. an RSS/XML feed -- read the string here and parse it themselves.
        public string GetContent(string url)
        {
            url = new Uri(url).GetLeftPart(UriPartial.Query); // Remove hash fragment
            if (this.UseProxy)
            {
                url = ProxyUrlBuilder.Wrap(url);
            }

            // Request and transparently decompress gzip/brotli. Some sites (e.g. Euronews) only serve their
            // full markup when the client advertises Accept-Encoding the way a real browser does; without it
            // they return a near-empty shell, which previously made the scraper silently see no article.
            using var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All };
            using var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", GlobalConstants.DefaultUserAgent);
            var request = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url) { Version = new Version(2, 0) }).GetAwaiter().GetResult();
            return request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        // Resolves a possibly-relative URL (href/src) against BaseUrl. Absolute URLs are returned unchanged,
        // relative ones are joined onto BaseUrl. Returns null for null/empty input. Using this instead of a
        // raw "this.BaseUrl + href" concatenation keeps providers working whether the site emits absolute or
        // relative links, which is a common cause of silent breakage when a site is redesigned.
        protected string MakeAbsoluteUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            url = url.Trim();
            return Uri.TryCreate(new Uri(this.BaseUrl), url, out var result) ? result.ToString() : url;
        }
    }
}
