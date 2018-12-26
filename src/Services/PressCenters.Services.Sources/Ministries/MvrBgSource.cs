namespace PressCenters.Services.Sources.Ministries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Parser.Html;

    public class MvrBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.mvr.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}press/актуална-информация/актуална-информация/актуално";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".article__list .article .article__description a.link--clear")
                .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl)).Distinct().ToList();
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            var address = $"{this.BaseUrl}press/актуална-информация/актуална-информация/актуално";
            var parser = new HtmlParser();
            var httpClient = new HttpClient();
            for (var i = 1; i < 100; i++)
            {
                Console.WriteLine(i);
                var response = httpClient.PostAsync(
                    address,
                    new FormUrlEncodedContent(
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("page_no", i.ToString()),
                            new KeyValuePair<string, string>("page_size", "24"),
                        })).GetAwaiter().GetResult();
                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var document = parser.Parse(content);
                var links = document.QuerySelectorAll(".article__list .article .article__description a.link--clear")
                    .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl)).Distinct().ToList();
                var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var titleElement = document.QuerySelector(".article__description h1");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var timeElement = document.QuerySelector(".article__description h5");
            var timeAsString = timeElement?.TextContent?.Trim();
            if (!DateTime.TryParseExact(timeAsString, "dd MMM yyyy", CultureInfo.GetCultureInfo("bg-BG"), DateTimeStyles.None, out var time))
            {
                timeElement = document.QuerySelector(".article__description .timestamp");
                timeAsString = timeElement?.TextContent?.Trim();
                time = DateTime.ParseExact(timeAsString, "dd MMMM yyyy", CultureInfo.GetCultureInfo("bg-BG"));
            }

            var imageElement = document.QuerySelector("#image_source");
            var imageUrl = imageElement?.GetAttribute("src") ?? $"/images/sources/mvr.bg.jpg";

            var contentElement = document.QuerySelector(".article__container");
            this.RemoveRecursively(contentElement, timeElement);
            this.RemoveRecursively(contentElement, document.QuerySelector(".article__container div.row"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".article__container script"));
            this.RemoveRecursively(contentElement, document.QuerySelector(".article__container h1")); // title
            this.RemoveRecursively(contentElement, document.QuerySelector(".article__container .pull-right"));
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
