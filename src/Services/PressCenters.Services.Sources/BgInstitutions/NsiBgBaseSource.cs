namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    public abstract class NsiBgBaseSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.nsi.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = this.GetNewsListUrl();
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".view-content .views-field-title a")
                .Select(x => x.Attributes["href"].Value).Select(x => this.NormalizeUrl(x, this.BaseUrl))
                .ToList();
            var news = links.Select(this.GetPublication).ToList();
            return news;
        }

        public override string ExtractIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            var uri = new Uri(url);
            var id = this.ChooseUrlSegmentForId(uri);
            id = id.Replace("/", string.Empty);
            return id;
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var title = document.QuerySelector("h1.page__title").TextContent.Trim();
            var imageAndContent = document.QuerySelectorAll("article .field-items .field-item");
            var imageUrl = this.GetImageUrl(imageAndContent);
            var content = this.GetContent(imageAndContent);

            var timeElement = document.QuerySelector(".submitted span");
            var time = DateTime.Parse(timeElement.Attributes["content"].Value);

            return new RemoteNews(title, content, time, imageUrl);
        }

        protected abstract string GetContent(IHtmlCollection<IElement> imageAndContent);

        protected abstract string GetImageUrl(IHtmlCollection<IElement> imageAndContent);

        protected abstract string ChooseUrlSegmentForId(Uri uri);

        protected abstract string GetNewsListUrl();
    }
}
