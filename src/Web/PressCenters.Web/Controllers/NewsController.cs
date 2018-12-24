namespace PressCenters.Web.Controllers
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Mapping;
    using PressCenters.Web.ViewModels.News;

    public class NewsController : BaseController
    {
        private const int ItemsPerPage = 10;

        private readonly IDeletableEntityRepository<News> newsRepository;

        public NewsController(IDeletableEntityRepository<News> newsRepository)
        {
            this.newsRepository = newsRepository;
        }

        public IActionResult List(int id)
        {
            if (id <= 0)
            {
                id = 1;
            }

            var skip = (id - 1) * ItemsPerPage;
            var news =
                this.newsRepository.All()
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .Skip(skip)
                    .Take(ItemsPerPage)
                    .To<NewsViewModel>()
                    .ToList();
            var newsCount = this.newsRepository.All().Count();
            var pagesCount = (int)Math.Ceiling(newsCount / (decimal)ItemsPerPage);
            var viewModel = new NewsListViewModel
                        {
                            News = news,
                            CurrentPage = id,
                            PagesCount = pagesCount,
                            NewsCount = newsCount,
                        };
            return this.View(viewModel);
        }

        public IActionResult ById(int id, string slug)
        {
            var news = this.newsRepository.All().Where(x => x.Id == id).To<NewsViewModel>().FirstOrDefault();
            if (news == null)
            {
                return this.NotFound();
            }

            return this.View(news);
        }
    }
}
