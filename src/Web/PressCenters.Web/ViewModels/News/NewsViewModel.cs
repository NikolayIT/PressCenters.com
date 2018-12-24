namespace PressCenters.Web.ViewModels.News
{
    using System;
    using System.Net;

    using PressCenters.Common;
    using PressCenters.Data.Models;
    using PressCenters.Services;
    using PressCenters.Services.Mapping;

    public class NewsViewModel : IMapFrom<News>
    {
        private readonly ISlugGenerator slugGenerator;

        public NewsViewModel()
        {
            this.slugGenerator = new SlugGenerator();
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ShortContent
        {
            get
            {
                var strippedContent = WebUtility.HtmlDecode(this.Content?.StripHtml() ?? string.Empty);
                return strippedContent?.Substring(0, Math.Min(230, strippedContent.Length)) + "...";
            }
        }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public string SourceShortName { get; set; }

        public string SourceName { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Url => $"/News/{this.Id}/{this.slugGenerator.GenerateSlug(this.Title)}";
    }
}
