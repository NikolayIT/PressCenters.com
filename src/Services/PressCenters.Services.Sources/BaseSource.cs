[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PressCenters.Services.Sources.Tests")]

namespace PressCenters.Services.Sources
{
    using System;
    using System.Collections.Generic;

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

        public RemoteNews GetPublication(string url)
        {
            var publication = this.ParseRemoteNews(url);

            // Title
            publication.Title = publication.Title?.Trim();

            // Post date
            if (publication.PostDate > DateTime.Now)
            {
                publication.PostDate = DateTime.Now;
            }

            // Original URL
            publication.OriginalUrl = url;

            // Image URL
            if (publication.ImageUrl?.StartsWith("/images/sources/") == false)
            {
                publication.ImageUrl = this.NormalizeUrl(publication.ImageUrl?.Trim(), this.BaseUrl)?.Trim();
            }

            // Remote ID
            publication.RemoteId = this.ExtractIdFromUrl(url);

            return publication;
        }

        internal abstract string ExtractIdFromUrl(string url);

        protected abstract RemoteNews ParseRemoteNews(string url);

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

        protected void RemoveRecursively(INode element, INode itemToRemove)
        {
            try
            {
                element.RemoveChild(itemToRemove);
            }
            catch (Exception)
            {
            }

            foreach (var node in element.ChildNodes)
            {
                this.RemoveRecursively(node, itemToRemove);
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
