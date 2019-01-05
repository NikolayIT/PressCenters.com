namespace PressCenters.Services.Sources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using AngleSharp;
    using AngleSharp.Dom;

    public abstract class BaseSource
    {
        protected BaseSource()
        {
            var configuration = Configuration.Default.WithDefaultLoader();
            this.BrowsingContext = AngleSharp.BrowsingContext.New(configuration);
        }

        public abstract string BaseUrl { get; }

        protected IBrowsingContext BrowsingContext { get; }

        public abstract IEnumerable<RemoteNews> GetLatestPublications();

        public virtual IEnumerable<RemoteNews> GetAllPublications()
        {
            return new List<RemoteNews>();
        }

        public RemoteNews GetPublication(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
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
            if (publication.ImageUrl?.StartsWith("/images/sources/") == false)
            {
                publication.ImageUrl = this.NormalizeUrl(publication.ImageUrl?.Trim(), this.BaseUrl)?.Trim();
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
                .Select(x => this.NormalizeUrl(x?.Attributes["href"]?.Value, this.BaseUrl))
                .Where(x => x?.Contains(urlShouldContain) == true).Distinct().ToList();
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        protected string NormalizeUrl(string url, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (!Uri.TryCreate(new Uri(baseUrl), url, out var result))
            {
                return url;
            }

            return result.ToString();
        }

        protected void RemoveRecursively(INode element, INode elementToRemove)
        {
            try
            {
                element.RemoveChild(elementToRemove);
            }
            catch (Exception)
            {
            }

            foreach (var node in element.ChildNodes)
            {
                this.RemoveRecursively(node, elementToRemove);
            }
        }

        protected void NormalizeUrlsRecursively(IElement element, string baseUrl)
        {
            if (element.Attributes["href"] != null)
            {
                element.SetAttribute("href", this.NormalizeUrl(element.Attributes["href"].Value, baseUrl));
            }

            if (element.Attributes["src"] != null)
            {
                element.SetAttribute("src", this.NormalizeUrl(element.Attributes["src"].Value, baseUrl));
            }

            foreach (var node in element.Children)
            {
                this.NormalizeUrlsRecursively(node, baseUrl);
            }
        }
    }
}
