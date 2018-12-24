namespace PressCenters.Services.Sources
{
    using System;

    public class RemoteNews
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public DateTime PostDate { get; set; }

        public string RemoteId { get; set; }
    }
}
