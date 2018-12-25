namespace PressCenters.Web.Controllers
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Web.ViewModels.Stats;

    public class StatsController : BaseController
    {
        private readonly IDeletableEntityRepository<News> newsRepository;

        public StatsController(IDeletableEntityRepository<News> newsRepository)
        {
            this.newsRepository = newsRepository;
        }

        public IActionResult Index()
        {
            // TODO: Add some caching here
            var byDateOfWeek = this.newsRepository.All().Select(x => x.CreatedOn.Date).ToList()
                .GroupBy(x => x.Date.DayOfWeek)
                .Select(g => new ByDayOfWeekViewModel { DayOfWeek = g.Key, Count = g.Count() }).ToList();
            var newsCount = this.newsRepository.All().Count();
            var newsToday = this.newsRepository.All().Count(x => x.CreatedOn.Date == DateTime.Today);
            var yesterday = DateTime.Today.AddDays(-1);
            var newsYesterday = this.newsRepository.All().Count(x => x.CreatedOn.Date == yesterday);
            var model = new IndexViewModel
                        {
                            NewsByDayOfWeek = byDateOfWeek,
                            NewsCount = newsCount,
                            NewsToday = newsToday,
                            NewsYesterday = newsYesterday,
                        };
            return this.View(model);
        }
    }
}
