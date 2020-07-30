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
    using PressCenters.Services.Sources.MainNews;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

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

                if (news == null)
                {
                    context.WriteLine($"Null news in \"{source.TypeName}\"");
                    continue;
                }

                if (lastNews?.Title == news.Title && lastNews?.ImageUrl == news.ImageUrl)
                {
                    // The last news has the same title
                    continue;
                }

                await this.SaveImageLocally(news.ImageUrl, source.Id, context);

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

        private async Task SaveImageLocally(string imageUrl, int sourceId, PerformContext context)
        {
            var filePath = this.webHostEnvironment.WebRootPath + "/images/mainnews/" + sourceId + ".png";
            var defaultFilePath = this.webHostEnvironment.WebRootPath + "/images/mainnews/default.png";
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                File.Copy(defaultFilePath, filePath, true);
                return;
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(120), };
            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.122 Safari/537.36");
            var result = await client.GetAsync(imageUrl);
            if (!result.IsSuccessStatusCode)
            {
                File.Copy(defaultFilePath, filePath, true);
                return;
            }

            try
            {
                var imageBytes = await result.Content.ReadAsByteArrayAsync();
                using var image = Image.Load(imageBytes);
                image.Mutate(
                    x => x.Resize(
                        new ResizeOptions
                            {
                                Mode = ResizeMode.Crop,
                                Size = new Size(150, 110),
                                Position = AnchorPositionMode.Center,
                            }));
                var tempPath = this.webHostEnvironment.WebRootPath + "/images/mainnews/" + Path.GetRandomFileName() + ".png";
                await using (var stream = File.OpenWrite(tempPath))
                {
                    image.SaveAsPng(stream);
                }

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
