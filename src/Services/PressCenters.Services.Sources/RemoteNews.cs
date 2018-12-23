namespace PressCenters.Sources
{
    using System;

    public class RemoteNews
    {
        public RemoteNews()
        {
        }

        public RemoteNews(string title, string shortContent, string content, string imageUrl, string originalUrl, DateTime postDate, string remoteId)
        {
            this.Title = title;
            this.ShortContent = shortContent;
            this.Content = content;
            this.ImageUrl = imageUrl;
            this.OriginalUrl = originalUrl;
            this.PostDate = postDate;
            this.RemoteId = remoteId;
        }

        public string Title { get; set; }

        public string ShortContent { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string OriginalUrl { get; set; }

        public DateTime PostDate { get; set; }

        public string RemoteId { get; set; }
    }
}
