namespace PressCenters.Worker.Tasks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services;
    using PressCenters.Services.Sources.MainNews;
    using PressCenters.Worker.Common;

    public class MainNewsGetterTask : BaseTask<MainNewsGetterTask.Input, MainNewsGetterTask.Output>
    {
        private readonly IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository;

        private readonly IDeletableEntityRepository<MainNews> mainNewsRepository;

        private readonly ILogger logger;

        public MainNewsGetterTask(
            IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository,
            IDeletableEntityRepository<MainNews> mainNewsRepository,
            ILoggerFactory loggerFactory)
        {
            this.mainNewsSourcesRepository = mainNewsSourcesRepository;
            this.mainNewsRepository = mainNewsRepository;
            this.logger = loggerFactory.CreateLogger<MainNewsGetterTask>();
        }

        protected override async Task<Output> DoWork(Input input)
        {
            var updated = 0;
            string errors = null;
            foreach (var source in this.mainNewsSourcesRepository.All().ToList())
            {
                var lastNews =
                    this.mainNewsRepository.All().Where(x => x.SourceId == source.Id)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();
                var instance = ReflectionHelpers.GetInstance<BaseMainNewsProvider>(source.TypeName);

                RemoteMainNews news = null;
                try
                {
                    news = instance.GetMainNews();
                }
                catch (Exception e)
                {
                    errors += $"Error in {source.TypeName}: {e.Message}; ";
                }

                if (news == null)
                {
                    errors += $"Null news in {source.TypeName}; ";
                    continue;
                }

                if (lastNews?.Title == news.Title && lastNews?.ImageUrl == news.ImageUrl)
                {
                    // The last news has the same title
                    this.logger.LogInformation($"Getting main news from {source.Name}. Nothing new.");
                    continue;
                }

                updated++;
                await this.mainNewsRepository.AddAsync(
                    new MainNews
                    {
                        Title = news.Title,
                        OriginalUrl = news.OriginalUrl,
                        ImageUrl = news.ImageUrl,
                        SourceId = source.Id,
                    });
                this.logger.LogInformation($"Getting main news from {source.Name}. New item added.");
            }

            await this.mainNewsRepository.SaveChangesAsync();
            return new Output { Updated = updated, Error = errors, Ok = errors == null };
        }

        protected override WorkerTask Recreate(WorkerTask currentTask, Input currentParameters, Output currentResult) =>
            new WorkerTask(currentTask, DateTime.UtcNow.AddSeconds(60));

        public class Input : BaseTaskInput
        {
        }

        public class Output : BaseTaskOutput
        {
            public int Updated { get; set; }
        }
    }
}
