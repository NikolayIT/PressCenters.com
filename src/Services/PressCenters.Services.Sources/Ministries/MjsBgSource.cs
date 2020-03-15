namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;

    using AngleSharp;
    using AngleSharp.Dom;

    using Newtonsoft.Json;

    using PressCenters.Common;

    /// <summary>
    /// Министерство на правосъдието.
    /// </summary>
    public class MjsBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.mjs.bg/";

        protected override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var newsData = this.ReadStringFromUrl("https://mjs.bg/api/content/GetNewsData?count=5&blockId=19&top=0");
            var newsList = JsonConvert.DeserializeObject<NewsList>(newsData);
            foreach (var news in newsList.Rows)
            {
                yield return this.GetPublication($"https://mjs.bg/home/index/{news.Url}");
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var html = document.ToHtml();

            var title = html.GetStringBetween("\"title\": {\n      \"bg\": \"", "\"");

            var time = DateTime.Now;

            var imageId = html.GetStringBetween("\"imageId\": \"", "\"");
            var imageUrl = string.IsNullOrWhiteSpace(imageId)
                               ? "/images/sources/mjs.bg.jpg"
                               : "https://mjs.bg/api/part/GetBlob?hash=" + imageId;

            var content = html.Replace("\\\"", "__QUOTE__").GetStringBetween("\"body\": {\n      \"bg\": \"", "\"")
                .Replace("__QUOTE__", "\"");

            return new RemoteNews(title, content, time, imageUrl);
        }

        public class NewsList
        {
            [JsonProperty("rows")]
            public Row[] Rows { get; set; }

            [JsonProperty("count")]
            public int Count { get; set; }
        }

        public class Row
        {
            [JsonProperty("blockId")]
            public int BlockId { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("jsonContent")]
            public string JsonContent { get; set; }

            [JsonProperty("json")]
            public object Json { get; set; }
        }
    }
}
