namespace PressCenters.Worker.Tasks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Worker.Common;

    public class NewsTagsTask : BaseTask<NewsTagsTask.Input, NewsTagsTask.Output>
    {
        private readonly IDeletableEntityRepository<News> newsRepository;

        private readonly INewsService newsService;

        private readonly ITagsService tagsService;

        private readonly ILogger logger;

        public NewsTagsTask(
            IDeletableEntityRepository<News> newsRepository,
            INewsService newsService,
            ITagsService tagsService,
            ILoggerFactory loggerFactory)
        {
            this.newsRepository = newsRepository;
            this.newsService = newsService;
            this.tagsService = tagsService;
            this.logger = loggerFactory.CreateLogger<MainNewsGetterTask>();
        }

        protected override async Task<Output> DoWork(Input input)
        {
            var allNews = this.newsRepository.AllWithDeleted().Where(x => x.Id > input.LastId).Take(5000).ToList();
            foreach (var news in allNews)
            {
                // Update tags
                await this.tagsService.UpdateTagsAsync(news.Id, news.Content);

                // Update search text
                news.SearchText = this.newsService.GetSearchText(news);
                await this.newsRepository.SaveChangesAsync();

                this.logger.LogInformation($"Tags for news {news.Id} updated.");
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
