namespace PressCenters.Web.Areas.Administration.Controllers
{
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Web.Areas.Administration.ViewModels.Dashboard;

    public class DashboardController : AdministrationController
    {
        private readonly IDeletableEntityRepository<News> newsRepository;

        private readonly IDeletableEntityRepository<ApplicationUser> usersRepository;

        private readonly IDbQueryRunner queryRunner;

        public DashboardController(
            IDeletableEntityRepository<News> newsRepository,
            IDeletableEntityRepository<ApplicationUser> usersRepository,
            IDbQueryRunner queryRunner)
        {
            this.newsRepository = newsRepository;
            this.usersRepository = usersRepository;
            this.queryRunner = queryRunner;
        }

        public IActionResult Index()
        {
            var viewModel = new IndexViewModel
                            {
                                UsersCount = this.usersRepository.All().Count(),
                                CountNullNewsImageUrls = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.ImageUrl)),
                                CountNullNewsOriginalUrl = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.OriginalUrl)),
                                CountNullNewsRemoteId = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.RemoteId)),
                            };
            return this.View(viewModel);
        }
    }
}
