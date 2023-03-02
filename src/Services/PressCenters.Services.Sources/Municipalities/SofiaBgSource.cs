namespace PressCenters.Services.Sources.Municipalities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;

    public class SofiaBgSource : BaseSource
    {
        public override string BaseUrl { get; } = "https://www.sofia.bg/";

        public override IEnumerable<RemoteNews> GetLatestPublications() =>
            this.GetPublications("news", ".portlet-body .row .row a", count: 5);

        public override IEnumerable<RemoteNews> GetAllPublications()
        {
            var newsCategories = new (string Url, string Instance)[]
                                     {
                                         ("news-archive-2016", "hultJKe9uK9Q"),
                                         ("news-archive-2017", "g0M0ZyT89hlj"),
                                         ("news-archive-2018", "TPmWufMJnWme"),
                                         ("novini-ot-2019", "Dl0IHiERLrsd"),
                                         ("news", "1ZlMReQfODHE"),
                                     };
            foreach (var newsCategory in newsCategories)
            {
                Console.WriteLine($"Category: {newsCategory}");
                for (var i = 1; i <= 100; i++)
                {
                    var url =
                        $"/web/guest/{newsCategory.Url}?p_p_id=101_INSTANCE_{newsCategory.Instance}&p_p_lifecycle=0&p_p_state=normal&p_p_mode=view&p_p_col_id=column-1&p_p_col_count=1&_101_INSTANCE_{newsCategory.Instance}_delta=6&_101_INSTANCE_{newsCategory.Instance}_keywords=&_101_INSTANCE_{newsCategory.Instance}_advancedSearch=false&_101_INSTANCE_{newsCategory.Instance}_andOperator=true&p_r_p_564233524_resetCur=false&_101_INSTANCE_{newsCategory.Instance}_cur={i}";
                    var news = this.GetPublications(url, ".news-wrapper .small-image a");
                    Console.WriteLine($"    Page {i} => {news.Count} news");

                    var document = new Lazy<IHtmlDocument>(() => this.Parser.ParseDocument(this.ReadStringFromUrl(this.BaseUrl + url)));
                    foreach (var remoteNews in news)
                    {
                        if (remoteNews.PostDate - DateTime.Now < new TimeSpan(0, 0, 1))
                        {
                            var titleElement = document.Value.QuerySelectorAll(".news-title")
                                .FirstOrDefault(x => x.TextContent.Trim() == remoteNews.Title);
                            var timeAsString = titleElement?.ParentElement?.QuerySelector(".date")?.TextContent?.Trim();
                            if (timeAsString == null)
                            {
                                Console.WriteLine(remoteNews.Title + " => NOT FOUND TIME!!!");
                                continue;
                            }

                            remoteNews.PostDate = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                        }

                        yield return remoteNews;
                    }

                    if (!news.Any())
                    {
                        break;
                    }
                }
            }
        }

        protected override RemoteNews ParseDocument(IDocument document, string url)
        {
            var componentParagraphs = document.QuerySelectorAll(".component-paragraph").ToList();
            var title = componentParagraphs[0].TextContent.Trim();

            var time = DateTime.Now;
            if (componentParagraphs.Count > 2)
            {
                var timeElement = componentParagraphs[2];
                var timeAsString = timeElement.TextContent.Trim();
                time = DateTime.ParseExact(timeAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            var imageElement = componentParagraphs[1].QuerySelector("img");
            var imageUrl = imageElement?.GetAttribute("src");

            var contentElement = componentParagraphs[1];
            contentElement.RemoveRecursively(imageElement);
            this.NormalizeUrlsRecursively(contentElement);
            var content = contentElement.InnerHtml.Trim();

            return new RemoteNews(title, content, time, imageUrl);
        }
    }
}
