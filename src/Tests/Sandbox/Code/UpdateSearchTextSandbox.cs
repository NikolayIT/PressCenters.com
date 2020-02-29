namespace Sandbox.Code
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;

    public class UpdateSearchTextSandbox
    {
        public async Task Work(IServiceProvider serviceProvider)
        {
            var newsService = serviceProvider.GetService<INewsService>();
            var newsRepository = serviceProvider.GetService<IDeletableEntityRepository<News>>();
            var allNews = newsRepository.AllWithDeleted().Count();
            for (var i = 0; i <= allNews / 1000; i++)
            {
                Console.WriteLine($"{i}/{allNews / 1000}");
                var news = newsRepository.AllWithDeleted().Skip(i * 1000).Take(1000);
                foreach (var newsItem in news)
                {
                    newsItem.SearchText = newsService.GetSearchText(newsItem);
                }

                await newsRepository.SaveChangesAsync();
            }
        }
    }
}
