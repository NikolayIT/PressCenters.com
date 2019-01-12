namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;
    using AngleSharp.Parser.Html;

    using Newtonsoft.Json;

    /// <summary>
    /// Министерство на правосъдието.
    /// </summary>
    public class MjsBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.mjs.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var news = new List<RemoteNews>();

            for (var i = 1; i <= 5; i++)
            {
                var remoteNews = this.GetNthLastNews(i);
                if (remoteNews == null)
                {
                    continue;
                }

                news.Add(remoteNews);
            }

            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 60; i++)
            {
                var remoteNews = this.GetNthLastNews(i);
                if (remoteNews == null)
                {
                    continue;
                }

                Console.WriteLine($"{i} => {remoteNews.Title}");
                yield return remoteNews;
            }

            foreach (var remoteNews in this.GetPublications("117/", ".navlist2 li a"))
            {
                yield return remoteNews;
            }
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector("div.lBorder ~ div.lTitle");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent;

            var timeElement = document.QuerySelector("div.lBorder ~div.lDate");
            var timeAsString = timeElement?.TextContent?.Trim();
            var time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var contentElement = document.QuerySelector("div.lBorder ~ div.lText");
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement?.InnerHtml;

            return new RemoteNews(title, content, time, "/images/sources/mjs.bg.jpg");
        }

        private RemoteNews GetNthLastNews(int i)
        {
            var json = this.ReadStringFromUrl($"{this.BaseUrl}News/GetJsonNews/?news={i}");
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var newsAsJson = JsonConvert.DeserializeObject<IEnumerable<NewsAsJson>>(json).FirstOrDefault();
            if (newsAsJson == null)
            {
                return null;
            }

            var parser = new HtmlParser();
            var url = parser.Parse(newsAsJson.Info)?.QuerySelector("a")?.Attributes["href"]?.Value?.TrimStart('/');
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            url = this.NormalizeUrl(url);
            var imageUrl = parser.Parse(newsAsJson.Img)?.QuerySelector("img")?.Attributes["src"]?.Value;
            var remoteNews = this.GetPublication(url);
            if (remoteNews == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                remoteNews.ImageUrl = this.NormalizeUrl(imageUrl);
            }

            return remoteNews;
        }

        private class NewsAsJson
        {
            public string Img { get; set; }

            public string Info { get; set; }
        }
    }
}
