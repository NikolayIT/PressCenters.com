namespace PressCenters.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using PressCenters.Services.Data.Images;

    using Xunit;

    public class LocalImageStorageTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "presscenters-tests-" + Guid.NewGuid().ToString("N"));

        [Fact]
        public async Task UploadShouldWriteTheFileUnderTheKeyPathAndExistsShouldSeeIt()
        {
            var storage = new LocalImageStorage(this.root);
            var key = NewsImagePaths.BigKey(123456);

            Assert.False(await storage.ExistsAsync(key));

            await storage.UploadAsync(key, new MemoryStream(Encoding.ASCII.GetBytes("webp")), NewsImagePaths.ContentType);

            Assert.True(await storage.ExistsAsync(key));
            Assert.Equal("webp", await File.ReadAllTextAsync(Path.Combine(this.root, "news", "456", "big_123456.webp")));
            Assert.Empty(Directory.GetFiles(Path.Combine(this.root, "news", "456"), "*.tmp"));
        }

        [Fact]
        public async Task UploadShouldOverwriteAnExistingFile()
        {
            var storage = new LocalImageStorage(this.root);
            var key = NewsImagePaths.SmallKey(7);

            await storage.UploadAsync(key, new MemoryStream(Encoding.ASCII.GetBytes("old")), NewsImagePaths.ContentType);
            await storage.UploadAsync(key, new MemoryStream(Encoding.ASCII.GetBytes("new")), NewsImagePaths.ContentType);

            Assert.Equal("new", await File.ReadAllTextAsync(Path.Combine(this.root, "news", "7", "small_7.webp")));
        }

        [Fact]
        public async Task ListKeysShouldReturnForwardSlashKeysUnderThePrefixOnly()
        {
            var storage = new LocalImageStorage(this.root);
            await storage.UploadAsync(NewsImagePaths.BigKey(1), new MemoryStream(new byte[1]), NewsImagePaths.ContentType);
            await storage.UploadAsync(NewsImagePaths.SmallKey(1), new MemoryStream(new byte[1]), NewsImagePaths.ContentType);
            await storage.UploadAsync("mainnews/1.png", new MemoryStream(new byte[1]), NewsImagePaths.ContentType);

            var keys = new List<string>();
            await foreach (var key in storage.ListKeysAsync(NewsImagePaths.Prefix))
            {
                keys.Add(key);
            }

            keys.Sort(StringComparer.Ordinal);
            Assert.Equal(new[] { "news/1/big_1.webp", "news/1/small_1.webp" }, keys);
        }

        [Fact]
        public async Task ListKeysShouldBeEmptyWhenTheRootDoesNotExist()
        {
            var storage = new LocalImageStorage(Path.Combine(this.root, "missing"));
            await foreach (var key in storage.ListKeysAsync(NewsImagePaths.Prefix))
            {
                Assert.Fail($"Unexpected key {key}");
            }
        }

        [Fact]
        public async Task DownloadShouldReturnTheUploadedBytesAndDeleteShouldRemoveThem()
        {
            var storage = new LocalImageStorage(this.root);
            var key = NewsImagePaths.BigKey(42);
            await storage.UploadAsync(key, new MemoryStream(new byte[] { 1, 2, 3 }), NewsImagePaths.ContentType);

            Assert.Equal(new byte[] { 1, 2, 3 }, await storage.DownloadAsync(key));

            await storage.DeleteAsync(key);

            Assert.False(await storage.ExistsAsync(key));
        }

        [Fact]
        public async Task DeleteShouldIgnoreAMissingKey()
        {
            var storage = new LocalImageStorage(this.root);

            await storage.DeleteAsync("news/1/big_1.png");

            Assert.False(await storage.ExistsAsync("news/1/big_1.png"));
        }

        public void Dispose()
        {
            if (Directory.Exists(this.root))
            {
                Directory.Delete(this.root, recursive: true);
            }
        }
    }
}
