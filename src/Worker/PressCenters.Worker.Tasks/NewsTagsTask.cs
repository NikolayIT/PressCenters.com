namespace PressCenters.Worker.Tasks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Sources;
    using PressCenters.Worker.Common;

    public class NewsTagsTask : BaseTask<NewsTagsTask.Input, NewsTagsTask.Output>
    {
        private readonly IDeletableEntityRepository<News> newsRepository;

        private readonly INewsService newsService;

        private readonly ILogger logger;

        public NewsTagsTask(
            IDeletableEntityRepository<News> newsRepository,
            INewsService newsService,
            ILoggerFactory loggerFactory)
        {
            this.newsRepository = newsRepository;
            this.newsService = newsService;
            this.logger = loggerFactory.CreateLogger<MainNewsGetterTask>();
        }

        protected override async Task<Output> DoWork(Input input)
        {
            var allNews = this.newsRepository.AllWithDeleted().Where(x => x.Id > input.LastId).Take(5000);
            foreach (var news in allNews)
            {
            }

            return new Output { LastId = allNews.Max(x => x.Id) };
        }

        protected override WorkerTask Recreate(WorkerTask currentTask, Input currentParameters, Output currentResult)
        {
            currentParameters.LastId = currentResult.LastId;
            return new WorkerTask(currentTask, JsonConvert.SerializeObject(currentParameters), DateTime.UtcNow.AddHours(1));
        }

        public class Input : BaseTaskInput
        {
            public int LastId { get; set; }
        }

        public class Output : BaseTaskOutput
        {
            public int LastId { get; set; }
        }
    }
}
