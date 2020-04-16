namespace PressCenters.Services.CronJobs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Hangfire;
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

        [AutomaticRetry(Attempts = 3)]
        public async Task Work(string typeName)
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
                await this.newsService.AddAsync(remoteNews, source.Id);
            }
        }
    }
}
