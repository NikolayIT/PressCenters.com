namespace Sandbox.Code
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;

    using PressCenters.Services.Data.Images;

    using Spectre.Console;

    /// <summary>
    /// Converts the PNG news thumbnails (news/{n}/big_{id}.png and small_{id}.png, the format until October 2026) to
    /// WebP files next to them (big_{id}.webp, small_{id}.webp) in the configured image storage: the R2 bucket when
    /// the "R2" section of appsettings.Production.json is filled in, otherwise the local folder (Images:LocalRoot).
    /// Resumable: a PNG that already has its WebP is skipped, so a run can be stopped with Ctrl+C and started again.
    /// Rollout: run it, deploy the code that links the .webp files, run it again for the PNGs created meanwhile and,
    /// once the site looks right, run it with IMAGES_WEBP_DELETE_PNG=1 to delete the PNGs that have a WebP.
    /// Environment: IMAGES_WEBP=1 selects this mode; IMAGES_WEBP_PARALLELISM (default 8); IMAGES_WEBP_PREFIX
    /// (default "news/", e.g. "news/42/" for a trial run); IMAGES_WEBP_LIMIT (convert at most that many);
    /// IMAGES_WEBP_DELETE_PNG=1 (asks for confirmation in a terminal). Never touches the database.
    /// </summary>
    public class ConvertThumbnailsToWebpSandbox
    {
        public const string ModeVariable = "IMAGES_WEBP";

        private const string FailuresFileName = "webp-conversion-failures.txt";

        private const int MaxAttempts = 3;

        public async Task Work(IServiceProvider serviceProvider)
        {
            var storage = serviceProvider.GetRequiredService<IImageStorage>();
            var target = storage.ToString();
            var parallelism = int.TryParse(Environment.GetEnvironmentVariable("IMAGES_WEBP_PARALLELISM"), out var p) && p > 0 ? p : 8;
            var prefix = Environment.GetEnvironmentVariable("IMAGES_WEBP_PREFIX") is { Length: > 0 } value ? value : NewsImagePaths.Prefix;
            var limit = int.TryParse(Environment.GetEnvironmentVariable("IMAGES_WEBP_LIMIT"), out var l) && l > 0 ? l : int.MaxValue;
            var deletePngs = Environment.GetEnvironmentVariable("IMAGES_WEBP_DELETE_PNG") == "1";
            var interactive = AnsiConsole.Profile.Capabilities.Interactive;
            if (deletePngs && interactive
                && !AnsiConsole.Confirm($"Delete the PNG thumbnails that have a WebP from [deepskyblue1]{Markup.Escape(target)}[/]?", false))
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
                var work = Run(storage, progress, prefix, parallelism, limit, deletePngs, cancellation.Token);
                if (interactive)
                {
                    await AnsiConsole.Live(WebpConversionDashboard.Render(progress, target, options))
                        .AutoClear(false)
                        .StartAsync(async context =>
                        {
                            while (!work.IsCompleted)
                            {
                                await Task.WhenAny(work, Task.Delay(250));
                                context.UpdateTarget(WebpConversionDashboard.Render(progress, target, options));
                            }
                        });
                }
                else
                {
                    Console.WriteLine($"Converting the PNG news thumbnails to WebP in {target} ({Markup.Remove(options)})");
                    while (!work.IsCompleted)
                    {
                        await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(5)));
                        Console.WriteLine(WebpConversionDashboard.StatusLine(progress));
                    }
                }

                await work;
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
                if (deletePngs && alreadyWebp.Count > 0)
                {
                    progress.StartDeleting(alreadyWebp.Count);
                    await Parallel.ForEachAsync(
                        alreadyWebp,
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
                            var webp = ImageThumbnails.Reencode(png, ThumbnailFormat.Webp);
                            await WithRetry(
                                async () =>
                                {
                                    using var stream = new MemoryStream(webp);
                                    await storage.UploadAsync(thumbnail.Webp, stream, NewsImagePaths.ContentType, token);
                                },
                                token);
                            if (deletePngs)
                            {
                                await WithRetry(() => storage.DeleteAsync(thumbnail.Png, token), token);
                                progress.AddDeleted();
                            }

                            progress.AddConverted(thumbnail.Png, png.Length, webp.Length);
                        }
                        catch (Exception e) when (!token.IsCancellationRequested)
                        {
                            progress.AddFailed(thumbnail.Png, e.Message);
                        }
                    });

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

        // "news/{n}/big_{id}.png" -> id (the key matched NewsImagePaths.WebpKeyForPng, so the id is all digits).
        private static long NewsId(string pngKey)
        {
            var name = Path.GetFileNameWithoutExtension(pngKey);
            return long.Parse(name[(name.IndexOf('_') + 1)..], CultureInfo.InvariantCulture);
        }
    }
}
