namespace PressCenters.Web.Controllers
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Common;
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

        public IActionResult List(int id, string search)
        {
            id = Math.Max(1, id);
            var skip = (id - 1) * ItemsPerPage;
            var query = this.newsRepository.All();
            var words = search?.Split(' ').Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 2).ToList();
            if (words != null)
            {
                foreach (var word in words)
                {
                    query = query.Where(x => x.SearchText.Contains(word));
                }
            }

            var news = query
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .Skip(skip)
                    .Take(ItemsPerPage)
                    .To<NewsViewModel>()
                    .ToList();
            var newsCount = query.Count();
            var pagesCount = (int)Math.Ceiling(newsCount / (decimal)ItemsPerPage);
            var viewModel = new NewsListViewModel
                        {
                            News = news,
                            CurrentPage = id,
                            PagesCount = pagesCount,
                            NewsCount = newsCount,
                            Search = search,
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
