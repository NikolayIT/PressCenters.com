namespace PressCenters.Services.Data
{
    using System.Collections.Generic;
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

        public IReadOnlyCollection<int> GetLatestMainNewsIds()
        {
            // Only the id: a scalar TOP(1) per source is one index seek. Selecting the whole latest entity instead
            // (x.MainNews.OrderByDescending(...).FirstOrDefault()) makes EF number every MainNews row with ROW_NUMBER()
            // and sort the whole table, nvarchar(max) columns included (~13 s and up to 8 GB of query memory per call).
            return this.mainNewsSourcesRepository.All()
                .Where(x => x.MainNews.Any())
                .Select(x => x.MainNews.OrderByDescending(n => n.Id).Select(n => n.Id).FirstOrDefault())
                .ToList();
        }
    }
}
