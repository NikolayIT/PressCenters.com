namespace PressCenters.Services.Sources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Parser.Html;

    public abstract class BaseSource
    {
        protected BaseSource()
        {
            var configuration = Configuration.Default.WithDefaultLoader();
            this.BrowsingContext = AngleSharp.BrowsingContext.New(configuration);
        }

        public abstract string BaseUrl { get; }

        public virtual Encoding Encoding => null;

        protected IBrowsingContext BrowsingContext { get; }

        public abstract IEnumerable<RemoteNews> GetLatestPublications();

        public virtual IEnumerable<RemoteNews> GetAllPublications()
        {
            return new List<RemoteNews>();
        }

        public RemoteNews GetPublication(string url)
        {
            IDocument document;
            if (this.Encoding == null)
            {
                document = this.BrowsingContext.OpenAsync(url).GetAwaiter().GetResult();
            }
            else
            {
                var parser = new HtmlParser();
                var webClient = new WebClient { Encoding = this.Encoding };
                var html = webClient.DownloadString(url);
                document = parser.Parse(html);
            }

            var publication = this.ParseDocument(document);
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
            publication.OriginalUrl = url?.Trim();

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

        public virtual string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url.Trim().Trim('/'));
            var lastSegment = uri.Segments[uri.Segments.Length - 1];
            return WebUtility.UrlDecode(lastSegment);
        }

        protected abstract RemoteNews ParseDocument(IDocument document);

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
