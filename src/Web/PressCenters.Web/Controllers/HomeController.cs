namespace PressCenters.Web.Controllers
{
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Mapping;
    using PressCenters.Web.ViewModels.Home;
    using PressCenters.Web.ViewModels.News;

    public class HomeController : BaseController
    {
        private readonly IDeletableEntityRepository<News> newsRepository;

        public HomeController(IDeletableEntityRepository<News> newsRepository)
        {
            this.newsRepository = newsRepository;
        }

        public IActionResult Index()
        {
            var news = this.newsRepository.All().OrderByDescending(x => x.CreatedOn).Take(10).To<NewsViewModel>();
            var viewModel = new IndexViewModel { News = news };
            return this.View(viewModel);
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => this.View();
    }
}
