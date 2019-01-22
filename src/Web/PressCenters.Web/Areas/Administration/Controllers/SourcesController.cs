namespace PressCenters.Web.Areas.Administration.Controllers
{
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Web.Areas.Administration.ViewModels.Sources;

    public class SourcesController : AdministrationController
    {
        private readonly IDeletableEntityRepository<Source> sourcesRepository;

        public SourcesController(IDeletableEntityRepository<Source> sourcesRepository)
        {
            this.sourcesRepository = sourcesRepository;
        }

        public IActionResult Index()
        {
            var sources = this.sourcesRepository.All().Select(
                x => new SourceInfo
                     {
                         Id = x.Id,
                         SourceUrl = x.Url,
                         Name = x.Name,
                         LastNewsId = x.News.OrderByDescending(n => n.CreatedOn).Select(n => n.Id).FirstOrDefault(),
                         LastNewsDate = x.News.OrderByDescending(n => n.CreatedOn).Select(n => n.CreatedOn).FirstOrDefault(),
                     }).OrderBy(x => x.LastNewsDate).ToList();
            var model = new IndexViewModel { Sources = sources };
            return this.View(model);
        }
    }
}
