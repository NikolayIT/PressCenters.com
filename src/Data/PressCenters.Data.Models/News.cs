namespace PressCenters.Data.Models
{
    using PressCenters.Data.Common.Models;

    public class News : BaseDeletableModel<int>
    {
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the short version of the content.
        /// When ShortContent is null => use Content and trim it.
        /// </summary>
        /// <value>
        /// The short version of the content.
        /// </value>
        public string ShortContent { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public int? SourceId { get; set; }

        public virtual Source Source { get; set; }

        public string RemoteId { get; set; }
    }
}
