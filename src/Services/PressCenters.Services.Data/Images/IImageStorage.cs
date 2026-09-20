namespace PressCenters.Services.Data.Images
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Object store for the generated news thumbnails. Keys are relative paths such as
    /// "news/123/big_123456.png" (see <see cref="NewsImagePaths"/>).
    /// </summary>
    public interface IImageStorage
    {
        Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        IAsyncEnumerable<string> ListKeysAsync(string prefix, CancellationToken cancellationToken = default);
    }
}
