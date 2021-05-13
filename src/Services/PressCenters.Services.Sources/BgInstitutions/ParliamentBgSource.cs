namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AngleSharp.Dom;

    using Newtonsoft.Json;

    /// <summary>
    /// Народно събрание на Република България.
    /// </summary>
    public class ParliamentBgSource : BaseSource
    {
        public override string BaseUrl => "https://www.parliament.bg/";

        public override bool UseProxy => true;

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var json = this.ReadStringFromUrl($"{this.BaseUrl}api/v1/front-news/bg/5");
            var newsAsJson = JsonConvert.DeserializeObject<IEnumerable<NewsResponse>>(json);
            var links = newsAsJson.Select(x => $"{this.BaseUrl}bg/news/ID/{x.Id}").ToList();
            if (!links.Any())
            {
                throw new Exception("No publications found.");
            }

            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 4625; i++)
            {
                var remoteNews = this.GetPublication($"{this.BaseUrl}bg/news/ID/{i}");
                if (remoteNews == null)
                {
                    continue;
                }

                Console.WriteLine($"№{i} => {remoteNews.PostDate.ToShortDateString()} => {remoteNews.Title}");
                yield return remoteNews;
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var id = this.ExtractIdFromUrl(url);
            var json = this.ReadStringFromUrl($"{this.BaseUrl}api/v1/news/bg/{id}");
            var newsAsJson = JsonConvert.DeserializeObject<NewsResponse>(json);
            if (newsAsJson == null)
            {
                return null;
            }

            return new RemoteNews(
                newsAsJson.Title,
                newsAsJson.Body,
                newsAsJson.Date,
                newsAsJson.Media?.File);
        }

        public class NewsResponse
        {
            [JsonProperty("M_News_id")]
            public int Id { get; set; }

            [JsonProperty("M_News_date")]
            public DateTime Date { get; set; }

            [JsonProperty("M_NewsL_title")]
            public string Title { get; set; }

            [JsonProperty("M_NewsL_body")]
            public string Body { get; set; }

            [JsonProperty("media")]
            public Media Media { get; set; }
        }

        public class Media
        {
            [JsonProperty("M_NewsMG_file")]
            public string File { get; set; }
        }
    }
}
