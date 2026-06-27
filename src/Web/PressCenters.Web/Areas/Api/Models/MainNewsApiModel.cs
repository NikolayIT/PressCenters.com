namespace PressCenters.Web.Areas.Api.Models
{
    using System;

    public class MainNewsApiModel
    {
        public string Type { get; set; }

        public string Title { get; set; }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public DateTime Time { get; set; }
    }
}
