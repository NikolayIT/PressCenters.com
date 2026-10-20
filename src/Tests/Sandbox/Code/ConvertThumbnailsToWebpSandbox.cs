namespace Sandbox.Code
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    using PressCenters.Common;
    using PressCenters.Data;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Data.Images;
    using PressCenters.Services.Sources;

    using Spectre.Console;

    /// <summary>
    /// Converts the PNG news thumbnails (news/{n}/big_{id}.png and small_{id}.png, the format until October 2026) to
    /// WebP files next to them (big_{id}.webp, small_{id}.webp) in the configured image storage: the R2 bucket when
    /// the "R2" section of appsettings.Production.json is filled in, otherwise the local folder (Images:LocalRoot).
    /// Resumable: a PNG that already has its WebP is skipped, so a run can be stopped with Ctrl+C and started again.
    /// Rollout (done in October 2026): deploy the code that links the .webp files, with the pages falling back to the PNG
    /// while the WebP is missing, then run this with IMAGES_WEBP_DELETE_PNG=1: it converts everything and then deletes
    /// the PNGs that have a WebP.
    /// A damaged PNG (one that does not decode, or is cut short) is made again from the article's original image
    /// instead, as for a new article. The image URLs come from the database (only read), which must be the production
    /// one: with any other, e.g. the local copy that DefaultConnection is on a dev box, the damaged PNGs are left and
    /// listed in the failures file, and a later run with the production database (SANDBOX_DATABASE=Production, as in the
    /// "(production database)" launch profile) makes them.
    /// Environment: IMAGES_WEBP=1 selects this mode; IMAGES_WEBP_PARALLELISM (default 8); IMAGES_WEBP_PREFIX
    /// (default "news/", e.g. "news/42/" for a trial run); IMAGES_WEBP_LIMIT (convert at most that many);
    /// IMAGES_WEBP_DELETE_PNG=1 (asks for confirmation in a terminal). Never writes to the database.
    /// </summary>
    public class ConvertThumbnailsToWebpSandbox
    {
        public const string ModeVariable = "IMAGES_WEBP";

        private const string FailuresFileName = "webp-conversion-failures.txt";

        private const int MaxAttempts = 3;

        // The original images are on the sources' sites (mostly government ones): a few at a time.
        private const int OriginalsParallelism = 4;

        public async Task Work(IServiceProvider serviceProvider)
        {
            var storage = serviceProvider.GetRequiredService<IImageStorage>();
            var target = storage.ToString();
            var connection = serviceProvider.GetRequiredService<ApplicationDbContext>().Database.GetDbConnection();
            var database = $"{connection.Database} on {connection.DataSource}";
            var parallelism = int.TryParse(Environment.GetEnvironmentVariable("IMAGES_WEBP_PARALLELISM"), out var p) && p > 0 ? p : 8;
            var prefix = Environment.GetEnvironmentVariable("IMAGES_WEBP_PREFIX") is { Length: > 0 } value ? value : NewsImagePaths.Prefix;
            var limit = int.TryParse(Environment.GetEnvironmentVariable("IMAGES_WEBP_LIMIT"), out var l) && l > 0 ? l : int.MaxValue;
            var deletePngs = Environment.GetEnvironmentVariable("IMAGES_WEBP_DELETE_PNG") == "1";
            var interactive = AnsiConsole.Profile.Capabilities.Interactive;
            if (deletePngs && interactive
                && !AnsiConsole.Confirm(
                    $"Convert, then delete the PNG thumbnails that have a WebP from [deepskyblue1]{Markup.Escape(target)}[/]?", false))
            {
                deletePngs = false;
            }

            var options = $"[grey]prefix[/] {Markup.Escape(prefix)}   [grey]parallelism[/] {parallelism}   [grey]limit[/] "
                          + (limit == int.MaxValue ? "none" : limit.ToString("N0", CultureInfo.InvariantCulture)) + "   "
                          + (deletePngs ? "[bold red]delete PNGs[/]" : "[grey]keep PNGs[/]");

            using var cancellation = new CancellationTokenSource();
            void OnCancel(object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                cancellation.Cancel();
            }

            Console.CancelKeyPress += OnCancel;
            try
            {
                var progress = new WebpConversionProgress();
                var work = Run(serviceProvider, storage, progress, prefix, parallelism, limit, deletePngs, cancellation.Token);
                if (interactive)
                {
                    await AnsiConsole.Live(WebpConversionDashboard.Render(progress, target, database, options))
                        .AutoClear(false)
                        .StartAsync(async context =>
                        {
                            while (!work.IsCompleted)
                            {
                                await Task.WhenAny(work, Task.Delay(250));
                                context.UpdateTarget(WebpConversionDashboard.Render(progress, target, database, options));
                            }
                        });
                }
                else
                {
                    Console.WriteLine(
                        $"Converting the PNG news thumbnails to WebP in {target}, database {database} ({Markup.Remove(options)})");
                    while (!work.IsCompleted)
                    {
                        await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(5)));
                        Console.WriteLine(WebpConversionDashboard.StatusLine(progress));
                    }
                }

                await work;
                if (progress.Note != null && !interactive)
                {
                    Console.WriteLine(progress.Note);
                }

                if (progress.Failures.Count > 0)
                {
                    var failuresFile = Path.Combine(Environment.CurrentDirectory, FailuresFileName);
                    await File.WriteAllLinesAsync(failuresFile, progress.Failures.OrderBy(x => x, StringComparer.Ordinal));
                    Console.WriteLine($"{progress.Failures.Count} failures written to {failuresFile}");
                }
            }
            finally
            {
                Console.CancelKeyPress -= OnCancel;
            }
        }

        private static async Task Run(
            IServiceProvider serviceProvider,
            IImageStorage storage,
            WebpConversionProgress progress,
            string prefix,
            int parallelism,
            int limit,
            bool deletePngs,
            CancellationToken cancellationToken)
        {
            try
            {
                var keys = new HashSet<string>(StringComparer.Ordinal);
                await foreach (var key in storage.ListKeysAsync(prefix, cancellationToken))
                {
                    keys.Add(key);
                    progress.SetListed(keys.Count);
                }

                // Newest news first: those are the ones people open.
                var thumbnails = keys
                    .Select(x => (Png: x, Webp: NewsImagePaths.WebpKeyForPng(x)))
                    .Where(x => x.Webp != null)
                    .OrderByDescending(x => NewsId(x.Png))
                    .ThenBy(x => x.Png, StringComparer.Ordinal)
                    .ToList();
                var pending = thumbnails.Where(x => !keys.Contains(x.Webp)).Take(limit).ToList();
                var alreadyWebp = thumbnails.Where(x => keys.Contains(x.Webp)).Select(x => x.Png).ToList();
                progress.Plan(pending.Count, alreadyWebp.Count);

                var parallelOptions = new ParallelOptions
                                      {
                                          MaxDegreeOfParallelism = parallelism,
                                          CancellationToken = cancellationToken,
                                      };
                var converted = new ConcurrentQueue<string>();
                var damaged = new ConcurrentQueue<DamagedThumbnail>();
                progress.StartConverting();
                await Parallel.ForEachAsync(
                    pending,
                    parallelOptions,
                    async (thumbnail, token) =>
                    {
                        try
                        {
                            byte[] png = null;
                            await WithRetry(async () => png = await storage.DownloadAsync(thumbnail.Png, token), token);
                            byte[] webp;
                            try
                            {
                                webp = ImageThumbnails.Reencode(png, ThumbnailFormat.Webp);
                            }
                            catch (InvalidDataException e)
                            {
                                damaged.Enqueue(new DamagedThumbnail(thumbnail.Png, e.Message));
                                progress.AddDamaged();
                                return;
                            }

                            await WithRetry(
                                async () =>
                                {
                                    using var stream = new MemoryStream(webp);
                                    await storage.UploadAsync(thumbnail.Webp, stream, NewsImagePaths.ContentType, token);
                                },
                                token);
                            converted.Enqueue(thumbnail.Png);
                            progress.AddConverted(thumbnail.Png, png.Length, webp.Length);
                        }
                        catch (Exception e) when (!token.IsCancellationRequested)
                        {
                            progress.AddConversionFailed(thumbnail.Png, e.Message);
                        }
                    });

                var recreated = new ConcurrentQueue<string>();
                if (!damaged.IsEmpty)
                {
                    await RecreateFromOriginals(serviceProvider, keys, damaged.ToList(), recreated, progress, cancellationToken);
                }

                // Only after all the conversions: a browser that asked for a WebP before it existed can get Cloudflare's
                // cached 404 for a few minutes and then still needs the PNG. A PNG without a WebP is never deleted.
                if (deletePngs)
                {
                    var pngs = alreadyWebp.Concat(converted).Concat(recreated).ToList();
                    progress.StartDeleting(pngs.Count);
                    await Parallel.ForEachAsync(
                        pngs,
                        parallelOptions,
                        async (png, token) =>
                        {
                            try
                            {
                                await WithRetry(() => storage.DeleteAsync(png, token), token);
                                progress.AddDeleted();
                            }
                            catch (Exception e) when (!token.IsCancellationRequested)
                            {
                                progress.AddFailed(png, "delete: " + e.Message);
                            }
                        });
                }

                progress.Finish(WebpConversionPhase.Done);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                progress.Finish(WebpConversionPhase.Stopped);
            }
            catch (Exception e)
            {
                progress.Finish(WebpConversionPhase.Failed, e.Message);
            }
        }

        // A damaged PNG thumbnail is made again from the article's original image, both sizes, as for a new article.
        // The image URLs come from the database, which has to be the one behind this storage: in another one (e.g. the
        // local development copy) the same ids are other articles, whose pictures would end up on these thumbnails. So
        // it is used only when most of the news that have thumbnails here are in it.
        private static async Task RecreateFromOriginals(
            IServiceProvider serviceProvider,
            HashSet<string> keys,
            IReadOnlyCollection<DamagedThumbnail> damaged,
            ConcurrentQueue<string> recreated,
            WebpConversionProgress progress,
            CancellationToken cancellationToken)
        {
            progress.StartRecreating();
            var damagedByNews = damaged.GroupBy(x => (int)NewsId(x.Png)).ToDictionary(x => x.Key, x => x.ToList());
            var originals = new Dictionary<int, (string ImageUrl, string TypeName)>();
            try
            {
                var news = serviceProvider.GetRequiredService<IDeletableEntityRepository<News>>();
                var connection = serviceProvider.GetRequiredService<ApplicationDbContext>().Database.GetDbConnection();
                var newsIds = (await news.AllAsNoTrackingWithDeleted().Select(x => x.Id).ToListAsync(cancellationToken))
                    .ToHashSet();
                var thumbnailIds = keys.Where(IsThumbnail).Select(x => (int)NewsId(x)).ToHashSet();
                var known = thumbnailIds.Count(newsIds.Contains);
                if (known * 2 < thumbnailIds.Count)
                {
                    var details = $"the database '{connection.Database}' on '{connection.DataSource}' has only"
                                  + $" {known.ToString("N0", CultureInfo.InvariantCulture)} of the"
                                  + $" {thumbnailIds.Count.ToString("N0", CultureInfo.InvariantCulture)} news that have"
                                  + " thumbnails here, so it is not the one behind this storage. Run again with the launch"
                                  + " profile \"Convert thumbnails to WebP and delete the PNGs (production database)\""
                                  + " (SANDBOX_DATABASE=Production) to make them";
                    NotRecreated("not the production database", details);
                    return;
                }

                foreach (var id in damagedByNews.Keys)
                {
                    var original = await news.AllAsNoTrackingWithDeleted()
                        .Where(x => x.Id == id)
                        .Select(x => new { x.ImageUrl, x.Source.TypeName })
                        .FirstOrDefaultAsync(cancellationToken);
                    if (original != null)
                    {
                        originals[id] = (original.ImageUrl, original.TypeName);
                    }
                }
            }
            catch (Exception e) when (!cancellationToken.IsCancellationRequested)
            {
                NotRecreated("the database could not be read", "the database could not be read: " + e.Message);
                return;
            }

            var newsService = serviceProvider.GetRequiredService<INewsService>();
            await Parallel.ForEachAsync(
                damagedByNews,
                new ParallelOptions { MaxDegreeOfParallelism = OriginalsParallelism, CancellationToken = cancellationToken },
                async (entry, token) =>
                {
                    string problem;
                    if (!originals.TryGetValue(entry.Key, out var original))
                    {
                        problem = "the news is not in the database";
                    }
                    else if (string.IsNullOrWhiteSpace(original.ImageUrl))
                    {
                        problem = "the news has no image URL";
                    }
                    else
                    {
                        try
                        {
                            problem = await newsService.SaveImageAsync(original.ImageUrl, entry.Key, UseProxy(original.TypeName))
                                ? null
                                : $"{original.ImageUrl} could not be downloaded";
                        }
                        catch (Exception e) when (!token.IsCancellationRequested)
                        {
                            problem = $"{original.ImageUrl}: {e.Message}";
                        }
                    }

                    foreach (var thumbnail in entry.Value)
                    {
                        if (problem == null)
                        {
                            recreated.Enqueue(thumbnail.Png);
                            progress.AddRecreated();
                        }
                        else
                        {
                            progress.AddNotRecreated(thumbnail.Png, $"{thumbnail.Damage} Not made from the original image: {problem}");
                        }
                    }
                });

            void NotRecreated(string reason, string details)
            {
                progress.SetNote($"The damaged PNGs were not made from the original images: {details}.");
                foreach (var thumbnail in damaged)
                {
                    progress.AddNotRecreated(thumbnail.Png, $"{thumbnail.Damage} Not made from the original image: {reason}");
                }
            }
        }

        // As the source's scraper downloads it; a source class that no longer exists downloads directly.
        private static bool UseProxy(string typeName)
        {
            try
            {
                return typeName != null && ReflectionHelpers.GetInstance<BaseSource>(typeName).UseProxy;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // A news thumbnail, old (PNG) or new (WebP).
        private static bool IsThumbnail(string key) =>
            NewsImagePaths.WebpKeyForPng(key) != null
            || (key.EndsWith(".webp", StringComparison.Ordinal)
                && NewsImagePaths.WebpKeyForPng(NewsImagePaths.PngKeyForWebp(key)) != null);

        // Transient storage errors (timeouts, throttling) get two more tries; a PNG that does not decode does not.
        private static async Task WithRetry(Func<Task> action, CancellationToken cancellationToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception) when (attempt < MaxAttempts && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken);
                }
            }
        }

        // "news/{n}/big_{id}.png" (or .webp) -> id (the key is a thumbnail's, so the id is all digits).
        private static long NewsId(string key)
        {
            var name = Path.GetFileNameWithoutExtension(key);
            return long.Parse(name[(name.IndexOf('_') + 1)..], CultureInfo.InvariantCulture);
        }

        private record DamagedThumbnail(string Png, string Damage);
    }
}
