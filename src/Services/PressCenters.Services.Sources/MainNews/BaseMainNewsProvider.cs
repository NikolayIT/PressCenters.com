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
            url = new Uri(url).GetLeftPart(UriPartial.Query); // Remove hash fragment
            if (this.UseProxy)
            {
                url = ProxyUrlBuilder.Wrap(url);
            }

            var parser = new HtmlParser();

            // Request and transparently decompress gzip/brotli. Some sites (e.g. Euronews) only serve their
            // full markup when the client advertises Accept-Encoding the way a real browser does; without it
            // they return a near-empty shell, which previously made the scraper silently see no article.
            using var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All };
            using var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", GlobalConstants.DefaultUserAgent);
            //// httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            //// httpClient.DefaultRequestHeaders.Add("accept-language", "bg,en-US;q=0.9,en;q=0.8");
            var request = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url) { Version = new Version(2, 0) }).GetAwaiter().GetResult();
            var html = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var document = parser.ParseDocument(html);
            return document;
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
