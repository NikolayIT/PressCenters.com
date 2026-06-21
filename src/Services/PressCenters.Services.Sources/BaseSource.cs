[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PressCenters.Services.Sources.Tests")]

namespace PressCenters.Services.Sources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    using PressCenters.Common;

    public abstract class BaseSource : ISource
    {
        protected BaseSource()
        {
            this.Parser = new HtmlParser();
        }

        public abstract string BaseUrl { get; }

        public virtual bool UseProxy => false;

        /// <summary>
        /// Gets or sets a value indicating whether to bypass the proxy relay even when <see cref="UseProxy"/>
        /// is true. The relay exists for the production server's blocked IP; some sites are reachable directly
        /// from other machines, so an operator (e.g. a backfill) can fetch directly -- faster and not subject
        /// to relay rate limiting -- by setting this.
        /// </summary>
        public bool DisableProxy { get; set; }

        protected virtual Encoding Encoding => null;

        protected virtual List<(string Header, string Value)> Headers { get; set; }

        protected HtmlParser Parser { get; }

        public abstract IEnumerable<RemoteNews> GetLatestPublications();

        public virtual IEnumerable<RemoteNews> GetAllPublications()
        {
            return new List<RemoteNews>();
        }

        public RemoteNews GetPublication(string url)
        {
            IHtmlDocument document;
            try
            {
                document = this.Parser.ParseDocument(this.ReadStringFromUrl(url));
            }
            catch (HttpRequestException)
            {
                // Any single-article fetch failure -- a 4xx/5xx, or a relay/connection-level error (timeout,
                // host not responding) -- should skip that one article, not abort the whole source. This keeps
                // a bulk backfill limping forward through flaky relays / rate limiting instead of dying on the
                // first hiccup. A systematic failure still shows up as an inserted=0 result.
                return null;
            }
            catch (TaskCanceledException)
            {
                // HttpClient request timeout.
                return null;
            }

            var publication = this.ParseDocument(document, url);
            if (publication == null)
            {
                return null;
            }

            // Title
            publication.Title = publication.Title?.Trim().Replace("  ", " ");

            // Content
            publication.Content = publication.Content?.Trim();

            // Post date
            if (publication.PostDate > DateTime.Now)
            {
                publication.PostDate = DateTime.Now;
            }

            if (publication.PostDate.Date == DateTime.UtcNow.Date && publication.PostDate.Hour == 0
                                                                  && publication.PostDate.Minute == 0)
            {
                publication.PostDate = DateTime.Now;
            }

            // Original URL
            publication.OriginalUrl = url.Trim();

            // Image URL
            if (publication.ImageUrl != null)
            {
                publication.ImageUrl = publication.ImageUrl.Trim();
                publication.ImageUrl = this.NormalizeUrl(publication.ImageUrl)?.Trim();
            }

            // Remote ID
            publication.RemoteId = this.ExtractIdFromUrl(url)?.Trim();

            return publication;
        }

        internal virtual string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            var lastSegment = uri.Segments[^1];
            return WebUtility.UrlDecode(lastSegment);
        }

        protected abstract RemoteNews ParseDocument(IDocument document, string url);

        protected IList<RemoteNews> GetPublications(string address, string anchorSelector, string urlShouldContain = "", int count = 0, bool throwOnEmpty = true)
        {
            var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}{address}"));
            var links = document.QuerySelectorAll(anchorSelector)
                .Select(x => this.NormalizeUrl(x?.Attributes["href"]?.Value))
                .Where(x => x?.Contains(urlShouldContain) == true).Distinct().ToList();

            if (count > 0)
            {
                links = links.Take(count).ToList();
            }

            if (!links.Any() && throwOnEmpty)
            {
                throw new Exception("No publications found.");
            }

            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        protected string GetUrlParameterValue(string url, string parameterName)
        {
            var matches = Regex.Matches(url, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            var parameters = matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value));
            return parameters[parameterName];
        }

        protected string ReadStringFromUrl(string url)
        {
            url = new Uri(url).GetLeftPart(UriPartial.Query); // Remove hash fragment

            // Request and transparently decompress gzip/brotli. Some sites (e.g. bnb.bg) only serve their
            // real markup when the client advertises Accept-Encoding the way a browser does; without it they
            // return a near-empty shell, which made the scraper see no content. Decompression happens before
            // any custom Encoding decode below, so windows-1251 sources are unaffected.
            string Fetch(string target)
            {
                using var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All };
                using var httpClient = new HttpClient(handler);

                // Prefer HTTP/2 like a browser. Some anti-bot setups reject HTTP/1.1 from non-browser clients
                // (e.g. bfunion.bg: 403 on HTTP/1.1, 200 on HTTP/2). RequestVersionOrLower falls back to 1.1
                // for servers that don't offer h2, so it's safe for the rest.
                httpClient.DefaultRequestVersion = HttpVersion.Version20;
                httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", GlobalConstants.DefaultUserAgent);
                if (this.Headers != null)
                {
                    foreach (var (header, value) in this.Headers)
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header, value);
                    }
                }

                using var response = httpClient.GetAsync(target).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                if (this.Encoding != null)
                {
                    var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    return this.Encoding.GetString(bytes);
                }

                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (!this.UseProxy || this.DisableProxy || GlobalConstants.ProxyHosts.Length == 0)
            {
                return Fetch(url);
            }

            // Relay egress IPs differ in which sites they can reach (some get 403/404-blocked), so try the
            // hosts in random order and fail over to the next on any error rather than giving up on the first.
            Exception lastError = null;
            foreach (var host in GlobalConstants.ProxyHosts.OrderBy(_ => Guid.NewGuid()))
            {
                try
                {
                    return Fetch(ProxyUrlBuilder.WrapWith(url, host));
                }
                catch (Exception error)
                {
                    lastError = error;
                }
            }

            throw lastError ?? new HttpRequestException("No relay host could fetch the URL.");
        }

        // TODO: Normalize using current url as base url instead of this.BaseUrl?
        protected string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (!Uri.TryCreate(new Uri(this.BaseUrl), url, out var result))
            {
                return url;
            }

            return result.ToString();
        }

        protected void NormalizeUrlsRecursively(IElement element)
        {
            if (element == null)
            {
                return;
            }

            if (element.Attributes["href"] != null)
            {
                element.SetAttribute("href", this.NormalizeUrl(element.Attributes["href"].Value));
            }

            if (element.Attributes["src"] != null)
            {
                element.SetAttribute("src", this.NormalizeUrl(element.Attributes["src"].Value));
            }

            foreach (var node in element.Children)
            {
                this.NormalizeUrlsRecursively(node);
            }
        }
    }
}
