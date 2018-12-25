namespace PressCenters.Web.Controllers
{
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Mapping;
    using PressCenters.Web.ViewModels.Sources;

    public class SourcesController : BaseController
    {
        private readonly IDeletableEntityRepository<Source> sourcesRepository;

        public SourcesController(IDeletableEntityRepository<Source> sourcesRepository)
        {
            this.sourcesRepository = sourcesRepository;
        }

        public ActionResult List()
        {
            var sources = this.sourcesRepository.All().OrderBy(x => x.Name).To<SourceViewModel>().ToList();
            return this.View(sources);
        }
    }
}
