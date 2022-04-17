namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp.Dom;

    using Newtonsoft.Json;

    /// <summary>
    /// Национална агенция за приходите.
    /// </summary>
    public class NapBgSource : BaseSource
    {
        public override string BaseUrl => "https://nra.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var json = this.ReadStringFromUrl($"{this.BaseUrl}customSearchWCM/query?context=nra.bg25863&libName=agency&saId=266cf85b-a315-4d1f-902b-180f72e9303f&atId=d89c331e-6555-473c-836d-2c5869933eb5&returnElements=category,image&filterByElements=&rPP=10&currentPage=1&rootPage=nra&returnProperties=title,publishDate&currentUrl=https%3A%2F%2Fnra.bg%2Fwps%2Fportal%2Fnra%2Factualno%2Factualno%2F&dateFormat=dd.MM.yyyy&ancestors=false&descendants=true&orderBy=publishDate&orderBy2=publishDate&orderBy3=title&sortOrder=false&searchTerm=&from=01.01.{DateTime.UtcNow.Year}&before=31.12.{DateTime.UtcNow.Year}&optionMeta=true&rand=0.707991665810006");
            var newsAsJson = JsonConvert.DeserializeObject<IEnumerable<NapBgSource.NewsItemResponse>>(json);
            var links = newsAsJson.Select(x => x.ContentUrl?.Url).Where(x => x != null).ToList();
            if (!links.Any())
            {
                throw new Exception("No publications found.");
            }

            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1.page-title");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var contentElement = document.QuerySelector("#richTextContainer");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, DateTime.Now, null);
        }

        public class NewsItemResponse
        {
            [JsonProperty("contentUrl")]
            public ContentUrl ContentUrl { get; set; }
        }

        public class ContentUrl
        {
            [JsonProperty("url")]
            public string Url { get; set; }
        }
    }
}
