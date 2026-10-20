namespace PressCenters.Services.Data.Images
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// File-system implementation used when R2 is not configured (local development, tests).
    /// Writes {root}/{key}; with root = wwwroot/images this is the pre-R2 layout.
    /// </summary>
    public class LocalImageStorage : IImageStorage
    {
        private readonly string rootPath;

        public LocalImageStorage(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            var filePath = this.GetPath(key);
            var directory = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directory);

            // Write to a temp file and move so a reader never sees a half-written image.
            var tempPath = Path.Combine(directory, Path.GetRandomFileName() + ".tmp");
            await using (var stream = File.Create(tempPath))
            {
                await content.CopyToAsync(stream, cancellationToken);
            }

            File.Move(tempPath, filePath, overwrite: true);
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(File.Exists(this.GetPath(key)));

        public Task<byte[]> DownloadAsync(string key, CancellationToken cancellationToken = default) =>
            File.ReadAllBytesAsync(this.GetPath(key), cancellationToken);

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            var filePath = this.GetPath(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<string> ListKeysAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (!Directory.Exists(this.rootPath))
            {
                yield break;
            }

            var files = Directory.EnumerateFiles(this.rootPath, "*", SearchOption.AllDirectories)
                .Select(x => Path.GetRelativePath(this.rootPath, x).Replace('\\', '/'))
                .Where(x => x.StartsWith(prefix ?? string.Empty));
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return file;
            }
        }

        public override string ToString() => $"local folder {this.rootPath}";

        private string GetPath(string key) => Path.Combine(this.rootPath, key.Replace('/', Path.DirectorySeparatorChar));
    }
}
