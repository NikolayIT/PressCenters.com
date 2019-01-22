namespace PressCenters.Web.Areas.Administration.ViewModels.Sources
{
    using System;

    public class SourceInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string SourceUrl { get; set; }

        public DateTime LastNewsDate { get; set; }

        public int LastNewsId { get; set; }
    }
}
