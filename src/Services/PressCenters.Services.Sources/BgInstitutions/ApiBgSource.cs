namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;

    public class ApiBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.api.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var address = $"{this.BaseUrl}index.php/bg/prescentar/novini";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".ccm-page-list .news-item a.news_more_link")
                .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl))
                .Where(x => x.Contains("bg/prescentar/novini")).Distinct().ToList();
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 1; i <= 650; i++)
            {
                var address = $"{this.BaseUrl}index.php/bg/prescentar/novini?ccm_paging_p_b606={i}";
                var document = this.BrowsingContext.OpenAsync(address).Result;
                var links = document.QuerySelectorAll(".ccm-page-list .news-item a.news_more_link")
                    .Select(x => this.NormalizeUrl(x.Attributes["href"].Value, this.BaseUrl))
                    .Where(x => x.Contains("bg/prescentar/novini")).Distinct().ToList();
                var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
                Console.WriteLine($"Page {i} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document)
        {
            var title = document.QuerySelector(".box h1").TextContent?.Trim();

            var contentElement = document.QuerySelector(".news-article");

            var timeNode = contentElement.ChildNodes[0];
            var time = DateTime.ParseExact(timeNode.TextContent, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture); // 24.01.2016 09:59

            var imageElement = document.QuerySelector(".news-article img");
            var imageUrl = imageElement?.GetAttribute("src");

            contentElement.RemoveChild(timeNode);
            this.RemoveRecursively(contentElement, imageElement);
            this.NormalizeUrlsRecursively(contentElement, this.BaseUrl);
            var content = contentElement.InnerHtml;

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
