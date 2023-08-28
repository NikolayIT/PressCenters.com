namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;
    using AngleSharp.Xml.Parser;

    using Newtonsoft.Json;

    using PressCenters.Common;

    /// <summary>
    /// Министерство на електронното управление.
    /// </summary>
    public class EgovGovernmentBgSource : BaseSource
    {
        public override string BaseUrl => "https://egov.government.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var parser = new XmlParser();
            var content = this.ReadStringFromUrl($"{this.BaseUrl}wps/contenthandler/!ut/p/digest!ilagdCvhfeyi2zOhtKv_2g/searchfeed/search?sortKey=effectivedate&queryLang=en&locale=bg&resultLang=bg&constraint=%7b%22type%22%3a%22field%22%2c%22id%22%3a%22authoringtemplate%22%2c%22values%22%3a%5b%22contentFromList%22%5d%7d&constraint=%7b%22type%22%3a%22field%22%2c%22id%22%3a%22keywords%22%2c%22values%22%3a%5b%22presscenternews%22%5d%7d&sortOrder=desc&rand=0.17753105456139728&query=*&scope=1649765877343&start=0&results=4");
            var document = parser.ParseDocument(content);
            var links = document.QuerySelectorAll("*").Where(x => x.TagName == "wplc:field" && x.GetAttribute("id") == "name").Select(x => this.NormalizeUrl("wps/portal/ministry-meu/press-center/news/" + x.TextContent)).Take(4);
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
