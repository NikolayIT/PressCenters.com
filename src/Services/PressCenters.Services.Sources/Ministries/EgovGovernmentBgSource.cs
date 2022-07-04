namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    using Newtonsoft.Json;
    using PressCenters.Common;

    /// <summary>
    /// Министерство на електронното управление.
    /// </summary>
    public class EgovGovernmentBgSource : BaseSource
    {
        public override string BaseUrl => "https://egov.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var json = this.ReadStringFromUrl($"{this.BaseUrl}customSearchWCM/query?context=egov.government.bg-2818&libName=content&saId=3d8d30c4-ca91-4275-9ac5-95e2ae72c8cf&atId=ffdb5a5f-c051-4adb-b2cf-a04f4181a0a1&returnElements=summary&filterByElements=&rootPage=ministry-meu&returnProperties=title,publishDate&rPP=20&currentPage=1&currentUrl=https%3A%2F%2Fegov.government.bg%2Fwps%2Fportal%2Fministry-meu%2Fpress-center%2Fnews%2F&dateFormat=dd.MM.yyyy&ancestors=false&descendants=true&orderBy=publishDate&orderBy2=publishDate&orderBy3=title&sortOrder=false&searchTerm=&from=&before=");
            var newsAsJson = JsonConvert.DeserializeObject<IEnumerable<EgovGovernmentBgSource.NewsItemResponse>>(json);
            var links = newsAsJson.Select(x => x.ContentUrl?.Url).Where(x => x != null).Take(5).ToList();
            if (!links.Any())
            {
                throw new Exception("No publications found.");
            }

            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector(".wpthemeContainer h2");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector("#publish-date");
            var timeAsString = timeElement?.InnerHtml?.GetStringBetween("Дата на публикуване:", "<br>")?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var imageElement = document.QuerySelector(".image-50 img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = document.QuerySelector("#body");
            this.NormalizeUrlsRecursively(contentElement);
            var resultsElement = document.QuerySelector(".result-entry-wrapper");
            this.NormalizeUrlsRecursively(resultsElement);
            var content = contentElement?.InnerHtml + resultsElement?.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
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
