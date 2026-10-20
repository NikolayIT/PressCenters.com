namespace PressCenters.Web.Controllers
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Mapping;
    using PressCenters.Web.ViewModels.News;

    public class NewsController : BaseController
    {
        private const int ItemsPerPage = 10;

        // Every word adds a nested Where with its own FREETEXT predicate. Unbounded, a long search string (a few
        // hundred words) overflowed the stack while EF Core compiled the query, which kills the whole process.
        private const int MaxSearchWords = 10;

        private readonly IDeletableEntityRepository<News> newsRepository;

        public NewsController(IDeletableEntityRepository<News> newsRepository)
        {
            this.newsRepository = newsRepository;
        }

        public IActionResult List(int id, string search)
        {
            // The upper bound keeps (id - 1) * ItemsPerPage from overflowing into a negative OFFSET (a SQL error).
            id = Math.Clamp(id, 1, (int.MaxValue / ItemsPerPage) + 1);
            var skip = (id - 1) * ItemsPerPage;
            var query = this.newsRepository.All();
            var words = search?.Split(' ').Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 2).Distinct().Take(MaxSearchWords).ToList();
            if (words != null)
            {
                foreach (var word in words)
                {
                    query = query.Where(c => EF.Functions.FreeText(c.SearchText, word));
                    //// query = query.Where(x => x.SearchText.Contains(word));
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
