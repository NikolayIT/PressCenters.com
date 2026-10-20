namespace Sandbox.Code
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using PressCenters.Services.Data.Images;

    /// <summary>
    /// One-time migration: copies the on-disk wwwroot/images/news folder into the configured image storage
    /// (Cloudflare R2) keeping the same relative layout ("news/{id % 1000}/big_{id}.png"). Resumable: keys
    /// already present in the bucket are skipped, so the run can be interrupted and restarted.
    /// Configured in the git-ignored appsettings.Production.json next to the exe, "R2" section:
    ///   AccountId / AccessKeyId / SecretAccessKey / BucketName  R2 credentials
    ///   UploadRoot         local folder to upload, e.g. C:\Web\presscenters.com\wwwroot\images\news (selects this mode
    ///                      unless IMAGES_WEBP=1 or IMAGES_BACKFILL=1 selects another one)
    ///   UploadParallelism  concurrent uploads (default 32).
    /// </summary>
    public class UploadImagesToR2Sandbox
    {
        public const string UploadRootKey = "R2:UploadRoot";

        private const int MaxAttempts = 3;

        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".gif"] = "image/gif",
            [".webp"] = "image/webp",
        };

        public async Task Work(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var rootPath = configuration[UploadRootKey];
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                Console.WriteLine($"{UploadRootKey} is not set or does not exist: '{rootPath}'");
                return;
            }

            var parallelism = int.TryParse(configuration["R2:UploadParallelism"], out var p) && p > 0 ? p : 32;
            var storage = serviceProvider.GetRequiredService<IImageStorage>();
            if (storage is LocalImageStorage)
            {
                Console.WriteLine("R2 is not configured (R2 section empty) -- refusing to copy the folder onto itself.");
                return;
            }

            var sw = Stopwatch.StartNew();

            Console.WriteLine($"Listing existing '{NewsImagePaths.Prefix}' keys in the bucket...");
            var existing = new HashSet<string>(StringComparer.Ordinal);
            await foreach (var key in storage.ListKeysAsync(NewsImagePaths.Prefix))
            {
                existing.Add(key);
                if (existing.Count % 50000 == 0)
                {
                    Console.WriteLine($"  {existing.Count} keys listed...");
                }
            }

            Console.WriteLine($"{existing.Count} keys already in the bucket ({sw.Elapsed:g}).");

            // Besides the generated PNG thumbnails the folder holds a few hand-placed images (e.g. news/custom/*.jpg)
            // referenced from article HTML, so take every file with a known image type, not only *.png.
            Console.WriteLine($"Scanning {rootPath}...");
            var pending = Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
                .Where(path => ContentTypes.ContainsKey(Path.GetExtension(path)))
                .Select(path => (Path: path, Key: NewsImagePaths.Prefix + Path.GetRelativePath(rootPath, path).Replace('\\', '/')))
                .Where(x => !existing.Contains(x.Key))
                .ToList();
            Console.WriteLine($"{pending.Count} files to upload ({sw.Elapsed:g}). Parallelism: {parallelism}.");

            var uploaded = 0;
            var failures = new ConcurrentBag<string>();
            await Parallel.ForEachAsync(
                pending,
                new ParallelOptions { MaxDegreeOfParallelism = parallelism },
                async (file, cancellationToken) =>
                {
                    if (await UploadWithRetry(storage, file.Path, file.Key, cancellationToken))
                    {
                        var done = Interlocked.Increment(ref uploaded);
                        if (done % 1000 == 0)
                        {
                            var rate = done / Math.Max(1, sw.Elapsed.TotalSeconds);
                            Console.WriteLine($"  {done}/{pending.Count} uploaded, {failures.Count} failed, {rate:F0} files/s, {sw.Elapsed:g}");
                        }
                    }
                    else
                    {
                        failures.Add(file.Path);
                    }
                });

            Console.WriteLine($"Done: {uploaded} uploaded, {pending.Count - uploaded - failures.Count} skipped, {failures.Count} failed in {sw.Elapsed:g}.");
            if (failures.Count > 0)
            {
                var failuresFile = Path.Combine(Path.GetDirectoryName(rootPath.TrimEnd('\\', '/')) ?? rootPath, "r2-upload-failures.txt");
                await File.WriteAllLinesAsync(failuresFile, failures.OrderBy(x => x));
                Console.WriteLine($"Failed paths written to {failuresFile}. Re-run to retry (already uploaded keys are skipped).");
            }
        }

        private static async Task<bool> UploadWithRetry(IImageStorage storage, string path, string key, CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    await using var stream = File.OpenRead(path);
                    await storage.UploadAsync(key, stream, ContentTypes[Path.GetExtension(path)], cancellationToken);
                    return true;
                }
                catch (Exception e) when (attempt < MaxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken);
                    Console.WriteLine($"  retry {attempt} for {key}: {e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"  FAILED {key}: {e.Message}");
                }
            }

            return false;
        }
    }
}
