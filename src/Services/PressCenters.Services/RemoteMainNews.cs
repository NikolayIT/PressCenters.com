namespace PressCenters.Services
{
    public class RemoteMainNews
    {
        public RemoteMainNews(string title, string originalUrl, string imageUrl)
        {
            this.Title = title;
            this.OriginalUrl = originalUrl;
            this.ImageUrl = imageUrl;
        }

        public string Title { get; set; }

        public string OriginalUrl { get; set; }

        public string ImageUrl { get; set; }
    }
}
