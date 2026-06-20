namespace Sandbox.Code
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Sources;

    public class GetAllNewsSandbox
    {
        public async Task Work(IServiceProvider serviceProvider)
        {
            var newsService = serviceProvider.GetService<INewsService>();
            var sourcesRepository = serviceProvider.GetService<IDeletableEntityRepository<Source>>();

            // Controlled entirely via environment variables so the same build can be pointed at a local test
            // database or production without editing code:
            //   BACKFILL_SOURCES        comma-separated TypeName substrings to run (required; nothing runs otherwise)
            //   BACKFILL_MAX_PER_SOURCE  cap on inserts per source (default: no cap) -- use for local smoke tests
            //   BACKFILL_STOP_EXISTING   stop a source after this many consecutive already-existing items
            //                            (default 40; 0 = never stop = full backfill). GetAllPublications yields
            //                            newest-first, so this fills a recent "tail" gap and stops once it reaches
            //                            news already in the DB. Set 0 to also fill old internal gaps.
            //   BACKFILL_IMAGES          "1" to also download+resize images locally (only useful on the prod box)
            var targets = (Environment.GetEnvironmentVariable("BACKFILL_SOURCES") ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (targets.Length == 0)
            {
                Console.WriteLine("BACKFILL_SOURCES is empty -- nothing to do.");
                return;
            }

            var maxPerSource = int.TryParse(Environment.GetEnvironmentVariable("BACKFILL_MAX_PER_SOURCE"), out var mx) ? mx : int.MaxValue;
            var stopExisting = int.TryParse(Environment.GetEnvironmentVariable("BACKFILL_STOP_EXISTING"), out var se) ? se : 40;
            var downloadImages = Environment.GetEnvironmentVariable("BACKFILL_IMAGES") == "1";

            // Politeness delay between articles (ms). Bulk bursts get the relay/target site to rate-limit
            // (502/403), so space the requests out for large backfills.
            var delayMs = int.TryParse(Environment.GetEnvironmentVariable("BACKFILL_DELAY_MS"), out var dm) ? dm : 0;

            // Where the resized thumbnails are written. Keep the on-disk layout identical to the server
            // (<webRoot>/images/news/<id%1000>/big_<id>.png) and against the production DB the ids match, so the
            // folder can be copied straight onto the prod web root.
            var webRoot = Environment.GetEnvironmentVariable("BACKFILL_WEBROOT") ?? @"C:\Web\presscenters.com\wwwroot";

            foreach (var source in sourcesRepository.All().ToList())
            {
                if (!targets.Any(t => source.TypeName.Contains(t)))
                {
                    continue;
                }

                var provider = ReflectionHelpers.GetInstance<BaseSource>(source.TypeName);
                Console.WriteLine($"=== {source.TypeName} (id {source.Id}) backfill start (stopExisting={stopExisting}, max={maxPerSource}, images={downloadImages}) ===");
                int inserted = 0, skipped = 0, errors = 0, consecutiveExisting = 0;
                try
                {
                    foreach (var remoteNews in provider.GetAllPublications())
                    {
                        int? newsId;
                        try
                        {
                            newsId = await newsService.AddAsync(remoteNews, source.Id);
                        }
                        catch (Exception e)
                        {
                            errors++;
                            Console.WriteLine($"  ADD ERROR ({remoteNews?.OriginalUrl}): {e.Message}");
                            continue;
                        }

                        if (newsId.HasValue)
                        {
                            inserted++;
                            consecutiveExisting = 0;
                            if (downloadImages && !string.IsNullOrWhiteSpace(remoteNews.ImageUrl))
                            {
                                try
                                {
                                    await newsService.SaveImageLocallyAsync(
                                        remoteNews.ImageUrl, newsId.Value, webRoot, provider.UseProxy);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"  IMAGE ERROR (#{newsId}): {e.Message}");
                                }
                            }

                            if (inserted % 25 == 0 || inserted <= 3)
                            {
                                var t = remoteNews.Title ?? string.Empty;
                                Console.WriteLine($"  +{inserted} [{remoteNews.PostDate:yyyy-MM-dd}] {t.Substring(0, Math.Min(60, t.Length))}");
                            }

                            if (inserted >= maxPerSource)
                            {
                                Console.WriteLine($"  reached BACKFILL_MAX_PER_SOURCE ({maxPerSource}) -> stop.");
                                break;
                            }
                        }
                        else
                        {
                            skipped++;
                            consecutiveExisting++;
                            if (stopExisting > 0 && consecutiveExisting >= stopExisting)
                            {
                                Console.WriteLine($"  reached {stopExisting} consecutive existing items -> caught up, stop.");
                                break;
                            }
                        }

                        if (delayMs > 0)
                        {
                            await Task.Delay(delayMs);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"  GETALL ERROR: {e.Message}");
                }

                Console.WriteLine($"=== {source.TypeName}: inserted={inserted}, skipped(existing)={skipped}, errors={errors} ===");
            }
        }
    }
}
