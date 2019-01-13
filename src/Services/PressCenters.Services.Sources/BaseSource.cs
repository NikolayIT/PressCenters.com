[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PressCenters.Services.Sources.Tests")]

namespace PressCenters.Services.Sources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Parser.Html;

    public abstract class BaseSource : ISource
    {
        protected BaseSource()
        {
            var configuration = Configuration.Default.WithDefaultLoader();
            this.BrowsingContext = AngleSharp.BrowsingContext.New(configuration);
        }

        public abstract string BaseUrl { get; }

        protected virtual Encoding Encoding => null;

        protected IBrowsingContext BrowsingContext { get; }

        public abstract IEnumerable<RemoteNews> GetLatestPublications();

        public virtual IEnumerable<RemoteNews> GetAllPublications()
        {
            return new List<RemoteNews>();
        }

        public RemoteNews GetPublication(string url)
        {
            var urlToLoad = new Uri(url).GetLeftPart(UriPartial.Query); // Remove hash fragment
            IDocument document;
            if (this.Encoding == null)
            {
                document = this.BrowsingContext.OpenAsync(urlToLoad).GetAwaiter().GetResult();
            }
            else
            {
                var parser = new HtmlParser();
                var html = this.ReadStringFromUrl(urlToLoad);
                document = parser.Parse(html);
            }

            var publication = this.ParseDocument(document, url);
            if (publication == null)
            {
                return null;
            }

            // Title
            publication.Title = publication.Title?.Trim();

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
            publication.ImageUrl = publication.ImageUrl?.Trim();
            if (publication.ImageUrl?.StartsWith("/images/sources/") == false)
            {
                publication.ImageUrl = this.NormalizeUrl(publication.ImageUrl)?.Trim();
            }

            // Remote ID
            publication.RemoteId = this.ExtractIdFromUrl(url)?.Trim();

            return publication;
        }

        internal virtual string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            var lastSegment = uri.Segments[uri.Segments.Length - 1];
            return WebUtility.UrlDecode(lastSegment);
        }

        protected abstract RemoteNews ParseDocument(IDocument document, string url);

        protected IList<RemoteNews> GetPublications(string address, string anchorSelector, string urlShouldContain = "")
        {
            address = $"{this.BaseUrl}{address}";
            var document = this.BrowsingContext.OpenAsync(address).GetAwaiter().GetResult();
            var links = document.QuerySelectorAll(anchorSelector)
                .Select(x => this.NormalizeUrl(x?.Attributes["href"]?.Value))
                .Where(x => x?.Contains(urlShouldContain) == true).Distinct().ToList();
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

            var webClient = new WebClient();
            if (this.Encoding != null)
            {
                webClient.Encoding = this.Encoding;
            }

            var html = webClient.DownloadString(url);
            return html;
        }

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
