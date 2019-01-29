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
            var allDates = this.newsRepository.All().Select(x => x.CreatedOn.Date).ToList();
            var byDateOfWeek = allDates.GroupBy(x => x.Date.DayOfWeek)
                .Select(g => new GroupByViewModel<DayOfWeek> { Group = g.Key, Count = g.Count() }).ToList();
            var byMonth = allDates.GroupBy(x => x.Date.Month)
                .Select(g => new GroupByViewModel<int> { Group = g.Key, Count = g.Count() }).ToList();
            var byYear = allDates.GroupBy(x => x.Date.Year)
                .Select(g => new GroupByViewModel<int> { Group = g.Key, Count = g.Count() }).ToList();
            var newsCount = this.newsRepository.All().Count();
            var newsToday = this.newsRepository.All().Count(x => x.CreatedOn.Date == DateTime.Today);
            var newsYesterday = this.newsRepository.All().Count(x => x.CreatedOn.Date == DateTime.Today.AddDays(-1));
            var newsTheDayBeforeYesterday = this.newsRepository.All().Count(x => x.CreatedOn.Date == DateTime.Today.AddDays(-2));
            var model = new IndexViewModel
                        {
                            NewsByDayOfWeek = byDateOfWeek,
                            NewsByMonth = byMonth,
                            NewsByYear = byYear,
                            NewsCount = newsCount,
                            NewsToday = newsToday,
                            NewsYesterday = newsYesterday,
                            NewsTheDayBeforeYesterday = newsTheDayBeforeYesterday,
                        };
            return this.View(model);
        }
    }
}
