namespace PressCenters.Services.CronJobs
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Hangfire;
    using Hangfire.Console;
    using Hangfire.Server;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Sources;

    public class GetLatestPublicationsJob
    {
        private readonly IDeletableEntityRepository<Source> sourcesRepository;

        private readonly INewsService newsService;

        public GetLatestPublicationsJob(
            IDeletableEntityRepository<Source> sourcesRepository,
            INewsService newsService)
        {
            this.sourcesRepository = sourcesRepository;
            this.newsService = newsService;
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

            // GetPublication returns null for an article it could not fetch or parse, and some sources yield its
            // result as is. Skip those instead of failing the whole run on the first one.
            var publications = instance.GetLatestPublications().Where(x => x != null).ToList();
            foreach (var remoteNews in publications)
            {
                var newsId = await this.newsService.AddAsync(remoteNews, source.Id);
                if (newsId.HasValue)
                {
                    context.WriteLine($"NEW: {remoteNews.Title}");
                    try
                    {
                        await this.newsService.SaveImageAsync(remoteNews.ImageUrl, newsId.Value, instance.UseProxy);
                    }
                    catch (Exception e) when (e is InvalidDataException || e is UriFormatException)
                    {
                        // The news is already saved; a malformed image URL or a file that is not a decodable image
                        // must not abort the rest of the batch (it would never be retried anyway).
                        context.WriteLine($"Image error ({remoteNews.ImageUrl}): {e.Message}");
                    }
                }
            }
        }
    }
}
