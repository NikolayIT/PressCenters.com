namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp.Dom;

    /// <summary>
    /// Българска народна банка.
    /// </summary>
    public class BnbBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://bnb.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications()
        {
            var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}AboutUs/PressOffice/POPressReleases/POPRDate/index.htm"));
            var links = document.QuerySelectorAll("#main h3 a")
                .Select(x => $"{this.BaseUrl}AboutUs/PressOffice/POPressReleases/POPRDate/{x?.Attributes["href"]?.Value}").Take(5).ToList();
            var news = links.Select(this.GetPublication).Where(x => x != null).ToList();
            return news;
        }

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            for (var year = 1998; year <= DateTime.UtcNow.Year; year++)
            {
                var document = this.Parser.ParseDocument(this.ReadStringFromUrl($"{this.BaseUrl}AboutUs/PressOffice/POPressReleases/POPRDate/index.htm?forYear={year}"));
                var links = document.QuerySelectorAll("#main h3 a")
                    .Select(x => $"{this.BaseUrl}AboutUs/PressOffice/POPressReleases/POPRDate/{x?.Attributes["href"]?.Value}").ToList();
                var news = links.Select(this.GetPublication).Where(x => x != null).ToList();

                Console.WriteLine($"Page {year} => {news.Count} news");
                foreach (var remoteNews in news)
                {
                    yield return remoteNews;
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var contentElement = document.QuerySelector("#main");
            contentElement.RemoveRecursively(contentElement.QuerySelector("p"));

            var timeAsString = contentElement.QuerySelector("p").TextContent.Trim();
            if (!DateTime.TryParseExact(timeAsString, "d MMMM yyyy г.", new CultureInfo("bg-BG"), DateTimeStyles.None, out var time))
            {
                return null;
            }

            contentElement.RemoveRecursively(contentElement.QuerySelector("p"));
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            var title = contentElement.QuerySelector("p").TextContent.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim().TrimEnd('.').Trim();
            if (title.Length > 350)
            {
                title = "Прессъобщение";
            }

            return new RemoteNews(title, content, time, null);
        }
    }
}
