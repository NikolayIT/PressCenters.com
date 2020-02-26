namespace PressCenters.Services.CronJobs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

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

        public async Task Work(string typeName)
        {
            var source = this.sourcesRepository.AllWithDeleted().FirstOrDefault(x => x.TypeName == typeName);
            if (source == null)
            {
                throw new Exception("Source type not found in the database");
            }

            var instance = ReflectionHelpers.GetInstance<BaseSource>(typeName);
            var publications = instance.GetLatestPublications().ToList();
            if (!publications.Any())
            {
                throw new Exception("GetLatestPublications() returned 0 results.");
            }

            var added = 0;
            foreach (var remoteNews in publications)
            {
                if (await this.newsService.AddAsync(remoteNews, source.Id))
                {
                    Console.WriteLine($"New news: {source.ShortName}: \"{remoteNews.Title}\"");
                    added++;
                }
            }

            Console.WriteLine(added);
        }
    }
}
