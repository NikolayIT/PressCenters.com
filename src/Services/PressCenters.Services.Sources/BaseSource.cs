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

        protected IBrowsingContext BrowsingContext { get; }

        public abstract IEnumerable<RemoteNews> GetLatestPublications();

        protected string NormalizeUrl(string url, string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (!Uri.TryCreate(new Uri(siteUrl), url, out var result))
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
