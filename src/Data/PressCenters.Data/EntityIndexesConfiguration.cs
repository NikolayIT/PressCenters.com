namespace PressCenters.Data
{
    using System.Linq;

    using Microsoft.EntityFrameworkCore;

    using PressCenters.Data.Common.Models;
    using PressCenters.Data.Models;

    internal static class EntityIndexesConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            // IDeletableEntity.IsDeleted index
            var deletableEntityTypes = modelBuilder.Model
                .GetEntityTypes()
                .Where(et => et.ClrType != null && typeof(IDeletableEntity).IsAssignableFrom(et.ClrType));
            foreach (var deletableEntityType in deletableEntityTypes)
            {
                modelBuilder.Entity(deletableEntityType.ClrType).HasIndex(nameof(IDeletableEntity.IsDeleted));
            }

            // The news lists page with WHERE IsDeleted = 0 ORDER BY CreatedOn DESC, Id DESC. Without this index every
            // page scanned and sorted the whole table (Content included) with huge memory grants.
            modelBuilder.Entity<News>()
                .HasIndex(x => new { x.IsDeleted, x.CreatedOn, x.Id })
                .IsDescending(false, true, true);

            // The latest main news of each source is a TOP(1) per source. With IsDeleted in the key it is a single
            // covering seek, not a backward scan past the other sources' rows (far back for a source that stopped
            // updating). Replaces the IX_MainNews_SourceId foreign key index.
            modelBuilder.Entity<MainNews>()
                .HasIndex(x => new { x.SourceId, x.IsDeleted });
        }
    }
}
