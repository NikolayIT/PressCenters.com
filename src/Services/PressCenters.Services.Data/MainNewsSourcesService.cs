namespace PressCenters.Services.Data
{
    using System.Linq;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;

    public class MainNewsSourcesService : IMainNewsSourcesService
    {
        private readonly IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository;

        public MainNewsSourcesService(IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository)
        {
            this.mainNewsSourcesRepository = mainNewsSourcesRepository;
        }

        public int Count()
        {
            return this.mainNewsSourcesRepository.All().Count();
        }
    }
}
