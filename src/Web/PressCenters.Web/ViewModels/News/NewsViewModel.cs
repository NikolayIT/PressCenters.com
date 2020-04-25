namespace PressCenters.Web.ViewModels.News
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    using AngleSharp;
    using AngleSharp.Html.Parser;

    using AutoMapper;

    using Ganss.XSS;

    using PressCenters.Common;
    using PressCenters.Data.Models;
    using PressCenters.Services;
    using PressCenters.Services.Mapping;

    public class NewsViewModel : IMapFrom<News>, IHaveCustomMappings
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
                var document = parser.ParseDocument(html);

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

        public string ShortContent => this.GetShortContent(235);

        public string ImageUrl { get; set; }

        public string SmallImageUrl =>
            this.ImageUrl == null ? this.SourceDefaultImageUrl : $"/images/news/{this.Id % 1000}/small_{this.Id}.png";

        public string BigImageUrl =>
            this.ImageUrl == null ? this.SourceDefaultImageUrl : $"/images/news/{this.Id % 1000}/big_{this.Id}.png";

        public string OriginalUrl { get; set; }

        public string RemoteId { get; set; }

        public string SourceName { get; set; }

        public string SourceShortName { get; set; }

        public string SourceDefaultImageUrl { get; set; }

        public string SourceUrl { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public string ShorterOriginalUrl
        {
            get
            {
                if (this.OriginalUrl == null)
                {
                    return string.Empty;
                }

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

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<News, NewsViewModel>().ForMember(
                m => m.Tags,
                opt => opt.MapFrom(x => x.Tags.Select(t => t.Tag.Name)));
        }

        public string GetShortContent(int maxLength)
        {
            // TODO: Extract as a service
            var htmlSanitizer = new HtmlSanitizer();
            var html = htmlSanitizer.Sanitize(this.Content);
            var strippedContent = WebUtility.HtmlDecode(html?.StripHtml() ?? string.Empty);
            strippedContent = strippedContent.Replace("\n", " ");
            strippedContent = strippedContent.Replace("\t", " ");
            strippedContent = Regex.Replace(strippedContent, @"\s+", " ").Trim();
            return strippedContent.Length <= maxLength ? strippedContent : strippedContent.Substring(0, maxLength) + "...";
        }
    }
}
