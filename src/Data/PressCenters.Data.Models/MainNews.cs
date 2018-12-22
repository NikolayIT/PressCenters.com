namespace PressCenters.Data.Models
{
    using PressCenters.Data.Common.Models;

    public class MainNews : BaseDeletableModel<int>
    {
        public string Title { get; set; }

        public string ShortTitle { get; set; }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public int MainNewsSourceId { get; set; }

        public virtual MainNewsSource MainNewsSource { get; set; }
    }
}
