namespace PressCenters.Services.Data.Tests
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Amazon.S3;
    using Amazon.S3.Model;

    using Moq;

    using PressCenters.Services.Data.Images;

    using Xunit;

    public class R2ImageStorageTests
    {
        [Fact]
        public async Task UploadShouldStoreTheContentTypeAndAnImmutableCacheControl()
        {
            var client = new Mock<IAmazonS3>();
            PutObjectRequest request = null;
            client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .Callback<PutObjectRequest, CancellationToken>((r, _) => request = r)
                .ReturnsAsync(new PutObjectResponse());

            await new R2ImageStorage(client.Object, "bucket").UploadAsync(
                NewsImagePaths.BigKey(1), new MemoryStream(new byte[1]), NewsImagePaths.ContentType);

            Assert.Equal("bucket", request.BucketName);
            Assert.Equal("news/1/big_1.webp", request.Key);
            Assert.Equal("image/webp", request.ContentType);
            Assert.Contains("immutable", request.Headers.CacheControl);
        }

        [Fact]
        public async Task DownloadShouldReturnTheObjectBytes()
        {
            var client = new Mock<IAmazonS3>();
            client.Setup(x => x.GetObjectAsync("bucket", "news/1/big_1.png", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse { ResponseStream = new MemoryStream(new byte[] { 1, 2, 3 }) });

            var bytes = await new R2ImageStorage(client.Object, "bucket").DownloadAsync("news/1/big_1.png");

            Assert.Equal(new byte[] { 1, 2, 3 }, bytes);
        }

        [Fact]
        public async Task DeleteShouldDeleteTheObjectFromTheBucket()
        {
            var client = new Mock<IAmazonS3>();

            await new R2ImageStorage(client.Object, "bucket").DeleteAsync("news/1/big_1.png");

            client.Verify(x => x.DeleteObjectAsync("bucket", "news/1/big_1.png", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
