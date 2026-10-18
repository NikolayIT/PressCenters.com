namespace PressCenters.Services.Data.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    using PressCenters.Data;
    using PressCenters.Data.Models;
    using PressCenters.Data.Repositories;

    using Xunit;

    public class MainNewsSourcesServiceTests
    {
        [Fact]
        public async Task GetLatestMainNewsIdsShouldReturnTheLatestNotDeletedMainNewsOfEachSource()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var dbContext = new ApplicationDbContext(options);
            var withNews = new MainNewsSource { Name = "With news" };
            var withDeletedLatest = new MainNewsSource { Name = "With deleted latest news" };
            var withoutNews = new MainNewsSource { Name = "Without news" };
            var deleted = new MainNewsSource { Name = "Deleted", IsDeleted = true };
            dbContext.MainNewsSources.AddRange(withNews, withDeletedLatest, withoutNews, deleted);
            dbContext.MainNews.AddRange(
                new MainNews { Id = 1, Source = withNews },
                new MainNews { Id = 3, Source = withNews },
                new MainNews { Id = 2, Source = withNews },
                new MainNews { Id = 4, Source = withDeletedLatest },
                new MainNews { Id = 5, Source = withDeletedLatest, IsDeleted = true },
                new MainNews { Id = 6, Source = deleted });
            await dbContext.SaveChangesAsync();

            var service = new MainNewsSourcesService(new EfDeletableEntityRepository<MainNewsSource>(dbContext));

            Assert.Equal(new[] { 3, 4 }, service.GetLatestMainNewsIds().OrderBy(x => x));
        }
    }
}
