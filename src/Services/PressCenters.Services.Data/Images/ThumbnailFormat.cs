namespace PressCenters.Services.Data.Images
{
    public enum ThumbnailFormat
    {
        /// <summary>Lossy WebP: the news thumbnails.</summary>
        Webp,

        /// <summary>Lossless PNG: the main-news tiles, whose URLs (also returned by the API) end in .png.</summary>
        Png,
    }
}
