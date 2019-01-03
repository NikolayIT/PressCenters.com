namespace PressCenters.Data.Models
{
    using PressCenters.Data.Common.Models;

    public class News : BaseDeletableModel<int>
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public int? SourceId { get; set; }

        public virtual Source Source { get; set; }

        // TODO: Required
        public string RemoteId { get; set; }
    }
}
