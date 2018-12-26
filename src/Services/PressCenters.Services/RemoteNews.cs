namespace PressCenters.Services
{
    using System;

    public class RemoteNews
    {
        public RemoteNews(string title, string content, DateTime date, string imageUrl)
        {
            this.Title = title;
            this.Content = content;
            this.PostDate = date;
            this.ImageUrl = imageUrl;
        }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public DateTime PostDate { get; set; }

        public string RemoteId { get; set; }
    }
}
