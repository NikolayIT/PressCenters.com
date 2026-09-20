namespace PressCenters.Services.Data.Images
{
    /// <summary>
    /// Single place for the thumbnail key layout. Keys equal the historical relative path under
    /// wwwroot/images, so the on-disk folder and the R2 bucket have an identical structure.
    /// </summary>
    public static class NewsImagePaths
    {
        public const string Prefix = "news/";

        public const string ContentType = "image/png";

        public static string BigKey(int newsId) => $"{Prefix}{newsId % 1000}/big_{newsId}.png";

        public static string SmallKey(int newsId) => $"{Prefix}{newsId % 1000}/small_{newsId}.png";

        public static string Url(string baseUrl, string key) => $"{baseUrl?.TrimEnd('/')}/{key}";
    }
}
