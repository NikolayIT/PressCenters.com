namespace PressCenters.Web.ViewModels.Home
{
    using System.Collections.Generic;

    using PressCenters.Web.ViewModels.News;

    public class IndexViewModel
    {
        public IEnumerable<NewsViewModel> News { get; set; }
    }
}
