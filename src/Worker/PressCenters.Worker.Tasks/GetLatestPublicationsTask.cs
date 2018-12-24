namespace PressCenters.Worker.Tasks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Sources;
    using PressCenters.Worker.Common;

    public class GetLatestPublicationsTask : BaseTask<GetLatestPublicationsTask.Input, GetLatestPublicationsTask.Output>
    {
        private readonly IDeletableEntityRepository<Source> sourcesRepository;

        private readonly IDeletableEntityRepository<News> newsRepository;

        private readonly ILogger logger;

        public GetLatestPublicationsTask(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this.sourcesRepository = serviceProvider.GetService<IDeletableEntityRepository<Source>>();
            this.newsRepository = serviceProvider.GetService<IDeletableEntityRepository<News>>();
            this.logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MainNewsGetterTask>();
        }

        protected override async Task<Output> DoWork(Input input)
        {
            var source = this.sourcesRepository.AllWithDeleted().FirstOrDefault(x => x.TypeName == input.TypeName);
            if (source == null)
            {
                return new Output { Ok = false, Error = "Source type not found in the database." };
            }

            var instance = ReflectionHelpers.GetInstance<BaseSource>(input.TypeName);
            var result = instance.GetLatestPublications();
            var added = 0;
            foreach (var remoteNews in result.News)
            {
                if (this.newsRepository.AllWithDeleted().Any(x => x.SourceId == source.Id && x.RemoteId == remoteNews.RemoteId))
                {
                    // Already exists
                    continue;
                }

                var news = new News
                           {
                               Title = remoteNews.Title,
                               ShortContent = remoteNews.ShortContent,
                               OriginalUrl = remoteNews.OriginalUrl,
                               ImageUrl = remoteNews.ImageUrl,
                               Content = remoteNews.Content,
                               CreatedOn = remoteNews.PostDate,
                               Source = source,
                               RemoteId = remoteNews.RemoteId,
                           };
                await this.newsRepository.AddAsync(news);
                this.logger.LogInformation($"New news: {source.ShortName}: \"{news.Title}\"");
                added++;
            }

            await this.newsRepository.SaveChangesAsync();
            return new Output { Added = added };
        }

        protected override WorkerTask Recreate(WorkerTask currentTask, Input parameters)
        {
            return new WorkerTask(currentTask, DateTime.UtcNow.AddMinutes(5));
        }

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
