namespace PressCenters.Services.CronJobs
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Hangfire;
    using Hangfire.Console;
    using Hangfire.Server;

    using Microsoft.AspNetCore.Hosting;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services;
    using PressCenters.Services.Data.Images;
    using PressCenters.Services.Sources.MainNews;

    public class MainNewsGetterJob
    {
        private readonly IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository;

        private readonly IDeletableEntityRepository<MainNews> mainNewsRepository;

        private readonly IWebHostEnvironment webHostEnvironment;

        public MainNewsGetterJob(
            IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository,
            IDeletableEntityRepository<MainNews> mainNewsRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            this.mainNewsSourcesRepository = mainNewsSourcesRepository;
            this.mainNewsRepository = mainNewsRepository;
            this.webHostEnvironment = webHostEnvironment;
        }

        [AutomaticRetry(Attempts = 2)]
        public async Task Work(PerformContext context)
        {
            foreach (var source in this.mainNewsSourcesRepository.All().ToList())
            {
                var lastNews =
                    this.mainNewsRepository.All().Where(x => x.SourceId == source.Id)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();
                var instance = ReflectionHelpers.GetInstance<BaseMainNewsProvider>(source.TypeName);
                context.WriteLine(source.TypeName);
                RemoteMainNews news;
                try
                {
                    news = instance.GetMainNews();
                }
                catch (Exception e)
                {
                    context.WriteLine($"Error in \"{source.TypeName}\": {e.Message}");
                    continue;
                }

                // Providers return an object with null fields when their selectors miss (an error page from the site,
                // changed markup) rather than null. Saving it would show an empty tile and replace the last good image.
                if (string.IsNullOrWhiteSpace(news?.Title))
                {
                    context.WriteLine($"No news (empty title) in \"{source.TypeName}\"");
                    continue;
                }

                // OriginalUrl is rendered as a link on every page and returned by the API, and HTML encoding does not
                // neutralize a "javascript:" URL taken from a scraped page, so keep only http(s) links.
                if (!Uri.TryCreate(news.OriginalUrl, UriKind.Absolute, out var originalUri)
                    || (originalUri.Scheme != Uri.UriSchemeHttp && originalUri.Scheme != Uri.UriSchemeHttps))
                {
                    news.OriginalUrl = null;
                }

                if (lastNews?.Title == news.Title && lastNews?.ImageUrl == news.ImageUrl)
                {
                    // The last news has the same title
                    continue;
                }

                try
                {
                    await this.SaveImageLocally(news.ImageUrl, source.Id, instance.UseProxy, context);
                }
                catch (Exception e)
                {
                    context.WriteLine($"Error in saving image for \"{source.TypeName}\": {e.Message}");
                    continue;
                }

                await this.mainNewsRepository.AddAsync(
                    new MainNews
                        {
                            Title = news.Title,
                            OriginalUrl = news.OriginalUrl,
                            ImageUrl = news.ImageUrl,
                            SourceId = source.Id,
                        });
                await this.mainNewsRepository.SaveChangesAsync();
            }
        }

        // The relays differ in which sites they can reach (see BaseSource.ReadStringFromUrl), so try them in random
        // order until one returns the image. Returns the last response (possibly unsuccessful) or throws the last
        // error. Like ProxyUrlBuilder.Wrap, goes direct when no relay is configured.
        private static async Task<HttpResponseMessage> GetThroughRelaysAsync(HttpClient client, string url)
        {
            var hosts = GlobalConstants.ProxyHosts.OrderBy(_ => Guid.NewGuid()).ToList();
            if (hosts.Count == 0)
            {
                return await client.GetAsync(url);
            }

            HttpResponseMessage response = null;
            for (var i = 0; i < hosts.Count; i++)
            {
                response?.Dispose();
                response = null;
                try
                {
                    response = await client.GetAsync(ProxyUrlBuilder.WrapWith(url, hosts[i]).Replace("+", "%20"));
                }
                catch (Exception) when (i < hosts.Count - 1)
                {
                    // Relay host unreachable -- try the next one.
                    continue;
                }

                if (response.IsSuccessStatusCode)
                {
                    break;
                }
            }

            return response;
        }

        private async Task SaveImageLocally(string imageUrl, int sourceId, bool useProxy, PerformContext context)
        {
            var filePath = this.webHostEnvironment.WebRootPath + "/images/mainnews/" + sourceId + ".png";
            var defaultFilePath = this.webHostEnvironment.WebRootPath + "/images/mainnews/default.png";
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                File.Copy(defaultFilePath, filePath, true);
                return;
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(120), };
            client.DefaultRequestHeaders.Add("User-Agent", GlobalConstants.DefaultUserAgent);
            using var result = useProxy ? await GetThroughRelaysAsync(client, imageUrl) : await client.GetAsync(imageUrl);
            if (!result.IsSuccessStatusCode)
            {
                File.Copy(defaultFilePath, filePath, true);
                return;
            }

            try
            {
                var imageBytes = await result.Content.ReadAsByteArrayAsync();
                var thumbnail = ImageThumbnails.CropToFill(imageBytes, 150, 110, ThumbnailFormat.Png);
                var tempPath = this.webHostEnvironment.WebRootPath + "/images/mainnews/" + Path.GetRandomFileName() + ".png";
                await File.WriteAllBytesAsync(tempPath, thumbnail);
                File.Move(tempPath, filePath, true);
            }
            catch (Exception e)
            {
                context.WriteLine($"Download image ({imageUrl}) failed: \"{e}\"");
                File.Copy(defaultFilePath, filePath, true);
            }
        }
    }
}
