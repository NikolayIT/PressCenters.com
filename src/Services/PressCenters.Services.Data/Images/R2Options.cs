namespace PressCenters.Services.Data.Images
{
    /// <summary>
    /// Cloudflare R2 settings, bound from the "R2" configuration section. When the section is not filled in
    /// the app falls back to <see cref="LocalImageStorage"/> so local development needs no bucket.
    /// </summary>
    public class R2Options
    {
        public const string SectionName = "R2";

        public string AccountId { get; set; }

        public string AccessKeyId { get; set; }

        public string SecretAccessKey { get; set; }

        public string BucketName { get; set; }

        /// <summary>
        /// Public base URL of the bucket (custom domain), e.g. https://images.presscenters.com. No trailing slash.
        /// </summary>
        public string PublicBaseUrl { get; set; }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(this.AccountId)
            && !string.IsNullOrWhiteSpace(this.AccessKeyId)
            && !string.IsNullOrWhiteSpace(this.SecretAccessKey)
            && !string.IsNullOrWhiteSpace(this.BucketName);
    }
}
