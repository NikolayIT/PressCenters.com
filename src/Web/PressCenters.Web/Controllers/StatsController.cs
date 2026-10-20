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

        public IActionResult Index(int? id)
        {
            var query = this.newsRepository.All();
            if (id.HasValue)
            {
                query = query.Where(x => x.SourceId == id);
            }

            // Count per day in SQL (a few thousand rows) and aggregate everything else from that, instead of
            // loading the date of every news item into memory and running four more counts over the table.
            var countsByDate = query.GroupBy(x => x.CreatedOn.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();
            var byDateOfWeek = countsByDate.GroupBy(x => x.Date.DayOfWeek)
                .Select(g => new GroupByViewModel<DayOfWeek> { Group = g.Key, Count = g.Sum(x => x.Count) }).ToList();
            var byMonth = countsByDate.GroupBy(x => x.Date.Month)
                .Select(g => new GroupByViewModel<int> { Group = g.Key, Count = g.Sum(x => x.Count) }).ToList();
            var byYear = countsByDate.GroupBy(x => x.Date.Year)
                .Select(g => new GroupByViewModel<int> { Group = g.Key, Count = g.Sum(x => x.Count) }).ToList();
            var today = DateTime.Today;
            var newsCount = countsByDate.Sum(x => x.Count);
            var newsToday = countsByDate.Where(x => x.Date == today).Sum(x => x.Count);
            var newsYesterday = countsByDate.Where(x => x.Date == today.AddDays(-1)).Sum(x => x.Count);
            var newsTheDayBeforeYesterday = countsByDate.Where(x => x.Date == today.AddDays(-2)).Sum(x => x.Count);
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
