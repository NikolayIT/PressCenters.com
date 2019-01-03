namespace PressCenters.Web.Areas.Administration.ViewModels.Dashboard
{
    using System.Collections.Generic;

    using PressCenters.Data.Models;

    public class IndexViewModel
    {
        public int CountNullNewsImageUrls { get; set; }

        public int CountNullNewsOriginalUrl { get; set; }

        public int CountNullNewsRemoteId { get; set; }

        public int NotProcessedTaskCount { get; set; }

        public IEnumerable<WorkerTask> LastWorkerTaskErrors { get; set; }

        public IEnumerable<WorkerTask> ProcessingWorkerTasks { get; set; }
    }
}
