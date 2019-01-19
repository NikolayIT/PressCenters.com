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
    using PressCenters.Services.Sources.MainNews;
    using PressCenters.Worker.Common;

    public class MainNewsGetterTask : BaseTask<MainNewsGetterTask.Input, MainNewsGetterTask.Output>
    {
        private readonly IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository;

        private readonly IDeletableEntityRepository<MainNews> mainNewsRepository;

        private readonly ILogger logger;

        public MainNewsGetterTask(IServiceProvider serviceProvider)
        {
            this.mainNewsSourcesRepository = serviceProvider.GetService<IDeletableEntityRepository<MainNewsSource>>();
            this.mainNewsRepository = serviceProvider.GetService<IDeletableEntityRepository<MainNews>>();
            this.logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MainNewsGetterTask>();
        }

        protected override async Task<Output> DoWork(Input input)
        {
            var updated = 0;
            foreach (var source in this.mainNewsSourcesRepository.All().ToList())
            {
                var lastNews =
                    this.mainNewsRepository.All().Where(x => x.SourceId == source.Id)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();
                var instance = ReflectionHelpers.GetInstance<BaseMainNewsProvider>(source.TypeName);
                var news = instance.GetMainNews();
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
            return new Output { Updated = updated };
        }

        protected override WorkerTask Recreate(WorkerTask currentTask, Input parameters) =>
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
