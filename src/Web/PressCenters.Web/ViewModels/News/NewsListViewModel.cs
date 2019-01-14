namespace PressCenters.Web.ViewModels.News
{
    using System.Collections.Generic;

    public class NewsListViewModel
    {
        public IEnumerable<NewsViewModel> News { get; set; }

        public int CurrentPage { get; set; }

        public int PagesCount { get; set; }

        public int NewsCount { get; set; }

        public int PreviousPage => this.CurrentPage == 1 ? 1 : this.CurrentPage - 1;

        public int NextPage => this.CurrentPage == this.PagesCount ? this.PagesCount : this.CurrentPage + 1;

        public string Search { get; set; }
    }
}
