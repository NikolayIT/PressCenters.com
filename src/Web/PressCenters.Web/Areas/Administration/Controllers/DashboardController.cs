namespace PressCenters.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Web.Areas.Administration.ViewModels.Dashboard;

    public class DashboardController : AdministrationController
    {
        private readonly IRepository<WorkerTask> workerTasksRepository;

        private readonly IDeletableEntityRepository<News> newsRepository;

        public DashboardController(
            IRepository<WorkerTask> workerTasksRepository,
            IDeletableEntityRepository<News> newsRepository)
        {
            this.workerTasksRepository = workerTasksRepository;
            this.newsRepository = newsRepository;
        }

        public IActionResult Index()
        {
            var viewModel = new IndexViewModel
                            {
                                CountNullNewsImageUrls = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.ImageUrl)),
                                CountNullNewsOriginalUrl = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.OriginalUrl)),
                                CountNullNewsRemoteId = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.RemoteId)),
                                NotProcessedTaskCount = this.workerTasksRepository.All().Count(x => !x.Processed),
                                LastWorkerTaskErrors = this.workerTasksRepository.All()
                                    .Where(x => x.Result.Contains("\"Ok\":false")).OrderByDescending(x => x.Id).Take(20).ToList(),
                                ProcessingWorkerTasks =
                                    this.workerTasksRepository.All().Where(x => x.Processing).ToList(),
                            };
            return this.View(viewModel);
        }

        public async Task<IActionResult> RemoveProcessing()
        {
            var processing = this.workerTasksRepository.All().Where(x => x.Processing).ToList();
            foreach (var workerTask in processing)
            {
                workerTask.Processing = false;
            }

            await this.workerTasksRepository.SaveChangesAsync();

            return this.RedirectToAction("Index");
        }
    }
}
