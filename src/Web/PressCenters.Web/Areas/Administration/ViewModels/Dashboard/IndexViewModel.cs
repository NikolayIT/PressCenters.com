namespace PressCenters.Web.Areas.Administration.ViewModels.Dashboard
{
    using System.Collections.Generic;

    using PressCenters.Data.Models;

    public class IndexViewModel
    {
        public int UsersCount { get; set; }

        public int CountNullNewsImageUrls { get; set; }

        public int CountNullNewsOriginalUrl { get; set; }

        public int CountNullNewsRemoteId { get; set; }
    }
}
