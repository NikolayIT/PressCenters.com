namespace PressCenters.Services.CronJobs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Hangfire;
    using Hangfire.Console;
    using Hangfire.Server;

    using Microsoft.AspNetCore.Hosting;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Sources;

    public class GetLatestPublicationsJob
    {
        private readonly IDeletableEntityRepository<Source> sourcesRepository;

        private readonly INewsService newsService;

        private readonly IWebHostEnvironment webHostEnvironment;

        public GetLatestPublicationsJob(
            IDeletableEntityRepository<Source> sourcesRepository,
            INewsService newsService,
            IWebHostEnvironment webHostEnvironment)
        {
            this.sourcesRepository = sourcesRepository;
            this.newsService = newsService;
            this.webHostEnvironment = webHostEnvironment;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Work(string typeName, PerformContext context)
        {
            var source = this.sourcesRepository.AllWithDeleted().FirstOrDefault(x => x.TypeName == typeName);
            if (source == null)
            {
                throw new Exception("Source type not found in the database");
            }

            var instance = ReflectionHelpers.GetInstance<BaseSource>(typeName);
            var publications = instance.GetLatestPublications().ToList();
            foreach (var remoteNews in publications)
            {
                var newsId = await this.newsService.AddAsync(remoteNews, source.Id);
                if (newsId.HasValue && remoteNews != null)
                {
                    context.WriteLine($"NEW: {remoteNews.Title}");
                    await this.newsService.SaveImageLocallyAsync(
                        remoteNews.ImageUrl,
                        newsId.Value,
                        this.webHostEnvironment.WebRootPath,
                        instance.UseProxy);
                }
            }
        }
    }
}
