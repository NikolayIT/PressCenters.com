namespace PressCenters.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Data.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Web.Areas.Administration.ViewModels.Dashboard;

    public class DashboardController : AdministrationController
    {
        private readonly IRepository<WorkerTask> workerTasksRepository;

        private readonly IDeletableEntityRepository<News> newsRepository;

        private readonly IDeletableEntityRepository<ApplicationUser> usersRepository;

        private readonly IDbQueryRunner queryRunner;

        public DashboardController(
            IRepository<WorkerTask> workerTasksRepository,
            IDeletableEntityRepository<News> newsRepository,
            IDeletableEntityRepository<ApplicationUser> usersRepository,
            IDbQueryRunner queryRunner)
        {
            this.workerTasksRepository = workerTasksRepository;
            this.newsRepository = newsRepository;
            this.usersRepository = usersRepository;
            this.queryRunner = queryRunner;
        }

        public IActionResult Index()
        {
            var viewModel = new IndexViewModel
                            {
                                UsersCount = this.usersRepository.All().Count(),
                                CountNullNewsImageUrls = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.ImageUrl)),
                                CountNullNewsOriginalUrl = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.OriginalUrl)),
                                CountNullNewsRemoteId = this.newsRepository.All().Count(x => string.IsNullOrWhiteSpace(x.RemoteId)),
                                NotProcessedTaskCount = this.workerTasksRepository.All().Count(x => !x.Processed),
                                ProcessedTaskCount = this.workerTasksRepository.All().Count(x => x.Processed),
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

        public async Task<IActionResult> RemoveProcessed()
        {
            await this.queryRunner.RunQueryAsync("DELETE FROM [WorkerTasks] WHERE [Processed] = 1");
            return this.RedirectToAction("Index");
        }
    }
}
