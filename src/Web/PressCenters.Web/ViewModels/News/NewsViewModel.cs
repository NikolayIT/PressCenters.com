namespace PressCenters.Web.ViewModels.News
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Text.RegularExpressions;

    using AngleSharp.Extensions;
    using AngleSharp.Parser.Html;

    using Ganss.XSS;

    using PressCenters.Common;
    using PressCenters.Data.Models;
    using PressCenters.Services;
    using PressCenters.Services.Mapping;
    using PressCenters.Services.Sources;

    public class NewsViewModel : IMapFrom<News>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string SanitizedContent
        {
            get
            {
                // Sanitize
                var htmlSanitizer = new HtmlSanitizer();
                htmlSanitizer.AllowedCssProperties.Remove("font-size");
                htmlSanitizer.AllowedSchemes.Add("mailto");
                htmlSanitizer.AllowDataAttributes = false;
                var html = htmlSanitizer.Sanitize(this.Content);

                // Parse document
                var parser = new HtmlParser();
                var document = parser.Parse(html);

                // Remove empty paragraphs
                var paragraphs = document.QuerySelectorAll("p");
                foreach (var paragraph in paragraphs)
                {
                    if (string.IsNullOrWhiteSpace(paragraph.TextContent) &&
                        paragraph.QuerySelector("img") == null)
                    {
                        document.RemoveRecursively(paragraph);
                    }
                }

                // Add .table class for tables
                var tables = document.QuerySelectorAll("table");
                foreach (var table in tables)
                {
                    table.ClassName += " table table-striped table-bordered table-hover table-sm";
                }

                // Clear font size
                var fontElements = document.QuerySelectorAll("font");
                foreach (var fontElement in fontElements)
                {
                    if (fontElement.HasAttribute("size"))
                    {
                        fontElement.RemoveAttribute("size");
                    }
                }

                return document.ToHtml();
            }
        }

        public string ShortContent
        {
            get
            {
                // TODO: Extract as a service
                const int MaxLength = 235;
                var strippedContent = WebUtility.HtmlDecode(this.Content?.StripHtml() ?? string.Empty);
                strippedContent = strippedContent.Replace("\n", " ");
                strippedContent = strippedContent.Replace("\t", " ");
                strippedContent = Regex.Replace(strippedContent, @"\s+", " ").Trim();
                return strippedContent.Length <= MaxLength
                           ? strippedContent
                           : strippedContent.Substring(0, MaxLength) + "...";
            }
        }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public string RemoteId { get; set; }

        public string SourceName { get; set; }

        public string SourceShortName { get; set; }

        public string SourceUrl { get; set; }

        public string ShorterOriginalUrl
        {
            get
            {
                if (this.OriginalUrl.Length <= 65)
                {
                    return this.OriginalUrl;
                }

                return $"{this.OriginalUrl.Substring(0, 30)}..{this.OriginalUrl.Substring(this.OriginalUrl.Length - 30, 30)}";
            }
        }

        public DateTime CreatedOn { get; set; }

        public string CreatedOnAsString =>
            this.CreatedOn.Hour == 0 && this.CreatedOn.Minute == 0
                ? this.CreatedOn.ToString("ddd, dd MMM yyyy", new CultureInfo("bg-BG"))
                : this.CreatedOn.ToString("ddd, dd MMM yyyy HH:mm", new CultureInfo("bg-BG"));

        public string Url => $"/News/{this.Id}/{new SlugGenerator().GenerateSlug(this.Title)}";
    }
}
