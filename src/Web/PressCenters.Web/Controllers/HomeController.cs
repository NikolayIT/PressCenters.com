namespace PressCenters.Web.Controllers
{
    using System;
    using System.Text;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Services.Data;
    using PressCenters.Web.ViewModels.Home;

    public class HomeController : BaseController
    {
        private readonly INewsService newsService;

        private readonly ISourcesService sourcesService;

        private readonly IMainNewsSourcesService mainNewsSourcesService;

        public HomeController(
            INewsService newsService,
            ISourcesService sourcesService,
            IMainNewsSourcesService mainNewsSourcesService)
        {
            this.newsService = newsService;
            this.sourcesService = sourcesService;
            this.mainNewsSourcesService = mainNewsSourcesService;
        }

        public IActionResult Index()
        {
            return this.RedirectToAction("List", "News");
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        public IActionResult Services()
        {
            return this.View(
                new ServicesViewModel
                {
                    NewsCount = this.newsService.Count(),
                    SourcesCount = this.sourcesService.Count(),
                    MainNewsSourcesCount = this.mainNewsSourcesService.Count(),
                });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => this.View();

        [HttpGet("robots.txt")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        public IActionResult RobotsTxt() =>
            this.Content("User-agent: *" + Environment.NewLine + "Disallow:");

        [HttpGet("ads.txt")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        public IActionResult AdsTxt() =>
            this.Content("google.com, pub-4541634013740958, DIRECT, f08c47fec0942fa0");
    }
}
