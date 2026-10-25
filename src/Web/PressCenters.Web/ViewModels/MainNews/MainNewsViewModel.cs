namespace PressCenters.Web.ViewModels.MainNews
{
    using System;

    using PressCenters.Data.Models;
    using PressCenters.Services.Data.Images;
    using PressCenters.Services.Mapping;

    public class MainNewsViewModel : IMapFrom<MainNews>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string LocalImageUrl => NewsImagePaths.MainNewsUrl(null, this.SourceId, this.CreatedOn);

        public string OriginalUrl { get; set; }

        public int SourceId { get; set; }

        public string SourceName { get; set; }

        public string SourceUrl { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
