namespace PressCenters.Services.Data.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    using Moq;

    using PressCenters.Data;
    using PressCenters.Data.Models;
    using PressCenters.Data.Repositories;
    using PressCenters.Services.Data.Images;

    using Xunit;

    public class NewsServiceTests
    {
        [Fact]
        public async Task AddAsyncShouldNotInsertTheSameRemoteIdTwiceWhenItHasSurroundingWhitespace()
        {
            await using var dbContext = CreateDbContext();
            var service = CreateService(dbContext);

            var firstId = await service.AddAsync(new RemoteNews("Title", "<p>Content</p>", DateTime.Now, null) { RemoteId = " 123 " }, 1);
            var secondId = await service.AddAsync(new RemoteNews("Title", "<p>Content</p>", DateTime.Now, null) { RemoteId = " 123 " }, 1);

            Assert.NotNull(firstId);
            Assert.Null(secondId);
            Assert.Equal("123", dbContext.News.Single().RemoteId);
        }

        [Fact]
        public async Task AddAsyncShouldInsertTheSameRemoteIdForAnotherSource()
        {
            await using var dbContext = CreateDbContext();
            var service = CreateService(dbContext);

            await service.AddAsync(new RemoteNews("Title", "<p>Content</p>", DateTime.Now, null) { RemoteId = "123" }, 1);
            var otherSourceId = await service.AddAsync(new RemoteNews("Title", "<p>Content</p>", DateTime.Now, null) { RemoteId = "123" }, 2);

            Assert.NotNull(otherSourceId);
            Assert.Equal(2, dbContext.News.Count());
        }

        [Fact]
        public void GetSearchTextShouldKeepDistinctLowercaseWordsOfTitleAndContentText()
        {
            var service = CreateService(null);

            var searchText = service.GetSearchText(
                new News { Title = "Новина: Тест", Content = "<p>Първи <b>тест</b>, втори-ред; a 42!</p>" });

            Assert.Equal("новина тест първи втори ред 42", searchText);
        }

        private static ApplicationDbContext CreateDbContext() =>
            new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        private static NewsService CreateService(ApplicationDbContext dbContext) =>
            new NewsService(
                dbContext == null ? null : new EfDeletableEntityRepository<News>(dbContext),
                new Mock<IImageStorage>().Object);
    }
}
