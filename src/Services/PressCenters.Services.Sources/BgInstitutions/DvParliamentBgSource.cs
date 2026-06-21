namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;

    using AngleSharp.Dom;

    /// <summary>
    /// Държавен вестник.
    /// </summary>
    public class DvParliamentBgSource : BaseSource
    {
        public override string BaseUrl => "https://dv.parliament.bg/";

        // The site began blocking the prod server's IP (~Feb 2026, when prod scraping stopped); the relay
        // reaches it and returns the same page, so route through it.
        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}DVWeb/index.faces"));
            var titleElement = document.QuerySelector(".tdHead1");
            var contentElement = document.QuerySelector("#index_form\\:dataTable1");
            if (titleElement == null || contentElement == null)
            {
                yield break;
            }

            var title = titleElement.TextContent.Trim();
            var images = contentElement.QuerySelectorAll("img");
            foreach (var image in images)
            {
                contentElement.RemoveRecursively(image);
            }

            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.OuterHtml;
            content = content.Replace("showMaterialDV.jsp", "DVWeb/showMaterialDV.jsp");
            var news = new RemoteNews(title, content, DateTime.Now, null)
            {
                RemoteId = title,
            };
            yield return news;
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            return null;
        }
    }
}
