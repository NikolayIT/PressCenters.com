namespace PressCenters.Web.ViewModels.MainNews
{
    using System;

    using PressCenters.Data.Models;
    using PressCenters.Services.Mapping;

    public class MainNewsViewModel : IMapFrom<MainNews>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string LocalImageUrl => $"/images/mainnews/{this.SourceId}.png";

        public string OriginalUrl { get; set; }

        public int SourceId { get; set; }

        public string SourceName { get; set; }

        public string SourceUrl { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
