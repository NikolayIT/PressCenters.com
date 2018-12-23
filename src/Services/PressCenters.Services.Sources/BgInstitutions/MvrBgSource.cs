namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using AngleSharp;

    public class MvrBgSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications(LocalPublicationsInfo localInfo)
        {
            var address = "http://press.mvr.bg/MoI/RssMvr.ashx?Id=6";
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links =
                document.QuerySelectorAll("item > link")
                    .Select(x => x.InnerHtml)
                    .Where(x => string.Compare(this.ExtractIdFromUrl(x), localInfo.LastLocalId, StringComparison.Ordinal) > 0)
                    .ToList();

            var news = links.Select(this.ParseRemoteNews).ToList();
            var remoteDataResult = new RemoteDataResult
                                       {
                                           News = news,
                                           LastNewsIdentifier =
                                               this.ExtractIdFromUrl(
                                                   news.OrderByDescending(x => x.PostDate)
                                                       .FirstOrDefault()?.OriginalUrl),
                                       };
            return remoteDataResult;
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var printDocument = this.BrowsingContext.OpenAsync(url + "?return=print").Result;
            var title = printDocument.QuerySelector("td.titleTXT")?.TextContent?.Trim();
            var shortContent = printDocument.QuerySelector("td.subtitleTXT")?.TextContent?.Trim();
            var content = printDocument.QuerySelector("td.simpleTXT")?.InnerHtml?.Trim();
            var time = DateTime.ParseExact(
                printDocument.QuerySelector("td.detailsTXT")?.TextContent?.Trim(),
                "Обновено HH:mm на dd MMMM yyyy г.",
                CultureInfo.GetCultureInfo("bg-BG"));
            var id = this.ExtractIdFromUrl(url);
            var expectedDate =
                DateTime.ParseExact(
                    id,
                    "yyMMdd_ss",
                    CultureInfo.InvariantCulture);
            if (time.Date != expectedDate.Date)
            {
                time = expectedDate.Date;
            }

            var mainNewsDocument = this.BrowsingContext.OpenAsync(url).Result;
            var imageUrl = mainNewsDocument.QuerySelector("#images > .image > img")?.GetAttribute("src");
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                imageUrl = this.NormalizeUrl(imageUrl, "http://press.mvr.bg/");
            }
            else
            {
                var pageWithImageUrl = url + "?ill=1";
                var pageWithImageUrlDocument = this.BrowsingContext.OpenAsync(pageWithImageUrl).Result;
                imageUrl =
                    this.NormalizeUrl(
                        pageWithImageUrlDocument.QuerySelector("#divIllustration > img")?.GetAttribute("src"), "http://press.mvr.bg/");
            }

            if (mainNewsDocument.QuerySelector("#related a") != null)
            {
                var additionalInfo = new StringBuilder();
                additionalInfo.AppendLine("<ul>");
                var links = mainNewsDocument.QuerySelectorAll("#related a");
                foreach (var link in links)
                {
                    var href = this.NormalizeUrl(link.Attributes["href"].Value, "http://press.mvr.bg/");
                    additionalInfo.AppendLine(
                        $"<a target=\"_blank\" href=\"{href}\">{link.TextContent}</a>");
                }

                additionalInfo.AppendLine("</ul>");

                content = $"<p>{shortContent}</p>{content}<div class=\"additionalInfo\">{additionalInfo}</div>";
            }

            var news = new RemoteNews
                           {
                               Title = title,
                               OriginalUrl = url,
                               ShortContent = string.IsNullOrWhiteSpace(shortContent) ? null : shortContent,
                               Content = content,
                               ImageUrl = imageUrl,
                               PostDate = time,
                               RemoteId = id,
                           };

            return news;
        }

        internal string ExtractIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            var lastSlashPosition = url.LastIndexOf("/", StringComparison.Ordinal) + 1;
            var lastUrlPart = url.Substring(lastSlashPosition, url.Length - lastSlashPosition);
            var id = lastUrlPart.Replace("news", string.Empty).Replace(".htm", string.Empty);
            return id;
        }
    }
}

// http://press.mvr.bg/default.htm?Category1Pg=231
// http://press.mvr.bg/News/archive.htm
