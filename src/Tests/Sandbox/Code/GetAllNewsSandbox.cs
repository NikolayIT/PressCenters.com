namespace Sandbox.Code
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Sources;

    public class GetAllNewsSandbox
    {
        public async Task Work(IServiceProvider serviceProvider)
        {
            var newsService = serviceProvider.GetService<INewsService>();
            var sourcesRepository = serviceProvider.GetService<IDeletableEntityRepository<Source>>();
            foreach (var source in sourcesRepository.All().ToList())
            {
                // Run only for selected sources
                if (!new[] { "SofiaBgSource" }.Any(x => source.TypeName.Contains(x)))
                {
                    continue;
                }

                var sourceProvider = ReflectionHelpers.GetInstance<BaseSource>(source.TypeName);
                Console.WriteLine($"Starting {source.TypeName}.GetAllPublications...");
                var news = sourceProvider.GetAllPublications();
                foreach (var remoteNews in news)
                {
                    await newsService.AddAsync(remoteNews, source.Id);
                }

                Console.WriteLine($"{source.TypeName}.GetAllPublications done.");
            }
        }
    }
}
