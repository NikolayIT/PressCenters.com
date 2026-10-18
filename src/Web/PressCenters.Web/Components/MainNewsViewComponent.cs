namespace PressCenters.Web.Components
{
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Mapping;
    using PressCenters.Web.ViewModels.MainNews;

    [ViewComponent(Name = "MainNews")]
    public class MainNewsViewComponent : ViewComponent
    {
        private readonly IDeletableEntityRepository<MainNews> mainNewsRepository;

        private readonly IMainNewsSourcesService mainNewsSourcesService;

        public MainNewsViewComponent(
            IDeletableEntityRepository<MainNews> mainNewsRepository,
            IMainNewsSourcesService mainNewsSourcesService)
        {
            this.mainNewsRepository = mainNewsRepository;
            this.mainNewsSourcesService = mainNewsSourcesService;
        }

        public IViewComponentResult Invoke()
        {
            var latestIds = this.mainNewsSourcesService.GetLatestMainNewsIds();
            var news = this.mainNewsRepository.All()
                .Where(x => latestIds.Contains(x.Id))
                .OrderByDescending(x => x.CreatedOn).To<MainNewsViewModel>().ToList();
            var viewModel = new MainNewsComponentViewModel { MainNews = news };
            return this.View(viewModel);
        }
    }
}
