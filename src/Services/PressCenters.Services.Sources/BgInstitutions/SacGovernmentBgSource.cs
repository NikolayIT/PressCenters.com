namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    public class SacGovernmentBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "http://www.sac.government.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("pages/bg/newsreel", "p a", "/news/", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var i = 2010; i <= 2022; i++)
            {
                var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}home.nsf/0/FF9D414158A0A77142258159004DE2BC?opendocument&year={i}"));
                var newsElements = document.QuerySelectorAll("p a");
                var newsCount = 0;
                foreach (var newsElement in newsElements)
                {
                    var url = this.NormalizeUrl(newsElement.Attributes["href"].Value);
                    if (!url.Contains("/news/"))
                    {
                        continue;
                    }

                    newsCount++;
                    var dateAsString = newsElement.TextContent.Trim();
                    var date = DateTime.ParseExact(dateAsString, "d.M.yyyy", CultureInfo.InvariantCulture);
                    var remoteNews = this.GetPublication(url);
                    if (remoteNews == null)
                    {
                        continue;
                    }

                    remoteNews.PostDate = date;
                    yield return remoteNews;
                }

                Console.WriteLine($"Year {i} => {newsCount} news");
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var titleElement = document.QuerySelector("h1");
            if (titleElement == null)
            {
                return null;
            }

            var title = titleElement.TextContent.Trim();

            var contentElement = document.QuerySelectorAll("td").OrderByDescending(x => x.TextContent.Length).FirstOrDefault();

            var imageElement = contentElement.QuerySelector("img");
            var imageUrl = imageElement?.GetAttribute("src");

            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, DateTime.Now, imageUrl);
        }
    }
}
