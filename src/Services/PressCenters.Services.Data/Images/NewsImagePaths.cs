namespace PressCenters.Services.Data.Images
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Single place for the thumbnail key layout. Keys equal the historical relative path under
    /// wwwroot/images, so the on-disk folder and the R2 bucket have an identical structure.
    /// </summary>
    public static class NewsImagePaths
    {
        public const string Prefix = "news/";

        public const string ContentType = "image/webp";

        // Until October 2026 the thumbnails were PNGs with the same names (ConvertThumbnailsToWebpSandbox converts them).
        private static readonly Regex PngThumbnailKey = new Regex(@"^news/\d+/(big|small)_\d+\.png$", RegexOptions.Compiled);

        public static string BigKey(int newsId) => $"{Prefix}{newsId % 1000}/big_{newsId}.webp";

        public static string SmallKey(int newsId) => $"{Prefix}{newsId % 1000}/small_{newsId}.webp";

        public static string Url(string baseUrl, string key) => $"{baseUrl?.TrimEnd('/')}/{key}";

        /// <summary>
        /// The address of a source's top-story thumbnail, which lives on the site (wwwroot/images/mainnews), not in the
        /// image bucket: <paramref name="siteUrl"/> is the site's address, or null for a path. Each new top story
        /// overwrites the source's file while Cloudflare and browsers keep a copy for hours, so the address carries the
        /// story's time and a new story is a new image to every cache. MainNewsGetterJob saves the file before the story.
        /// </summary>
        public static string MainNewsUrl(string siteUrl, int sourceId, DateTime createdOn) =>
            $"{siteUrl?.TrimEnd('/')}/images/mainnews/{sourceId}.png?t={createdOn.ToBinary().ToString(CultureInfo.InvariantCulture)}";

        /// <summary>
        /// The key of the WebP thumbnail that replaces an old PNG one ("news/{n}/big_{id}.png" or
        /// "news/{n}/small_{id}.png"), or null for any other key.
        /// </summary>
        public static string WebpKeyForPng(string key) =>
            key != null && PngThumbnailKey.IsMatch(key) ? Path.ChangeExtension(key, ".webp") : null;

        /// <summary>
        /// The key of the old PNG thumbnail that a WebP key replaces.
        /// </summary>
        public static string PngKeyForWebp(string key) => Path.ChangeExtension(key, ".png");
    }
}
