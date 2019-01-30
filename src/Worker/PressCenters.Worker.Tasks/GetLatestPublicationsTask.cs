namespace PressCenters.Worker.Tasks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Sources;
    using PressCenters.Worker.Common;

    public class GetLatestPublicationsTask : BaseTask<GetLatestPublicationsTask.Input, GetLatestPublicationsTask.Output>
    {
        private readonly IDeletableEntityRepository<Source> sourcesRepository;

        private readonly INewsService newsService;

        private readonly ILogger logger;

        public GetLatestPublicationsTask(
            IDeletableEntityRepository<Source> sourcesRepository,
            INewsService newsService,
            ILoggerFactory loggerFactory)
        {
            this.sourcesRepository = sourcesRepository;
            this.newsService = newsService;
            this.logger = loggerFactory.CreateLogger<MainNewsGetterTask>();
        }

        protected override async Task<Output> DoWork(Input input)
        {
            var source = this.sourcesRepository.AllWithDeleted().FirstOrDefault(x => x.TypeName == input.TypeName);
            if (source == null)
            {
                return new Output { Ok = false, Error = "Source type not found in the database." };
            }

            var instance = ReflectionHelpers.GetInstance<BaseSource>(input.TypeName);
            var publications = instance.GetLatestPublications().ToList();
            if (!publications.Any())
            {
                return new Output { Ok = false, Error = "GetLatestPublications() returned 0 results." };
            }

            var added = 0;
            foreach (var remoteNews in publications)
            {
                if (await this.newsService.AddAsync(remoteNews, source.Id))
                {
                    this.logger.LogInformation($"New news: {source.ShortName}: \"{remoteNews.Title}\"");
                    added++;
                }
            }

            return new Output { Added = added };
        }

        protected override WorkerTask Recreate(WorkerTask currentTask, Input parameters) =>
            new WorkerTask(currentTask, DateTime.UtcNow.AddSeconds(150));

        public class Input : BaseTaskInput
        {
            public string TypeName { get; set; }
        }

        public class Output : BaseTaskOutput
        {
            public int Added { get; set; }
        }
    }
}
