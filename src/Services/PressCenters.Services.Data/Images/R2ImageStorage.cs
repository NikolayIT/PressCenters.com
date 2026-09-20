namespace PressCenters.Services.Data.Images
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;

    /// <summary>
    /// Cloudflare R2 via its S3-compatible API. The client is thread-safe; register as a singleton.
    /// </summary>
    public class R2ImageStorage : IImageStorage
    {
        // Thumbnails are keyed by news id and never change, so browsers and the Cloudflare edge may cache forever.
        private const string CacheControl = "public, max-age=31536000, immutable";

        private readonly IAmazonS3 client;

        private readonly string bucketName;

        public R2ImageStorage(R2Options options)
            : this(CreateClient(options), options.BucketName)
        {
        }

        public R2ImageStorage(IAmazonS3 client, string bucketName)
        {
            this.client = client;
            this.bucketName = bucketName;
        }

        public static IAmazonS3 CreateClient(R2Options options)
        {
            var config = new AmazonS3Config
                             {
                                 ServiceURL = $"https://{options.AccountId}.r2.cloudflarestorage.com",
                                 ForcePathStyle = true,

                                 // SDK v4 defaults to CRC-based flexible checksums that R2 does not accept.
                                 RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                                 ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
                             };
            return new AmazonS3Client(new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey), config);
        }

        public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            var request = new PutObjectRequest
                              {
                                  BucketName = this.bucketName,
                                  Key = key,
                                  InputStream = content,
                                  ContentType = contentType,
                                  AutoCloseStream = false,
                                  DisablePayloadSigning = true,
                              };
            request.Headers.CacheControl = CacheControl;
            await this.client.PutObjectAsync(request, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.client.GetObjectMetadataAsync(this.bucketName, key, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async IAsyncEnumerable<string> ListKeysAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var request = new ListObjectsV2Request { BucketName = this.bucketName, Prefix = prefix, MaxKeys = 1000, };
            ListObjectsV2Response response;
            do
            {
                response = await this.client.ListObjectsV2Async(request, cancellationToken);
                foreach (var s3Object in response.S3Objects ?? new List<S3Object>())
                {
                    yield return s3Object.Key;
                }

                request.ContinuationToken = response.NextContinuationToken;
            }
            while (response.IsTruncated == true);
        }
    }
}
