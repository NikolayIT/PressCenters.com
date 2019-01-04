namespace PressCenters.Web.ViewModels.Stats
{
    using System.Collections.Generic;

    public class IndexViewModel
    {
        public IEnumerable<ByDayOfWeekViewModel> NewsByDayOfWeek { get; set; }

        public IEnumerable<ByYearViewModel> NewsByYear { get; set; }

        public int NewsCount { get; set; }

        public int NewsToday { get; set; }

        public int NewsYesterday { get; set; }

        public int NewsTheDayBeforeYesterday { get; set; }

        public int SourcesCount { get; set; }
    }
}
