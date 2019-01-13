namespace PressCenters.Services.Data
{
    using System.Linq;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;

    public class SourcesService : ISourcesService
    {
        private readonly IDeletableEntityRepository<Source> sourcesRepository;

        public SourcesService(IDeletableEntityRepository<Source> sourcesRepository)
        {
            this.sourcesRepository = sourcesRepository;
        }

        public int Count()
        {
            return this.sourcesRepository.All().Count();
        }
    }
}
