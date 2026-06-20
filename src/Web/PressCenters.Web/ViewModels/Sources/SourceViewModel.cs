namespace PressCenters.Web.ViewModels.Sources
{
    using System.Linq;

    using PressCenters.Data.Models;
    using PressCenters.Services.Mapping;

    public class SourceViewModel : IMapFrom<Source>, IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public int NewsCount { get; set; }

        public void CreateMappings(Mapster.TypeAdapterConfig configuration)
        {
            // NewsCount is a collection-count flattening. Map it explicitly via Count() so the
            // projection translates to a SQL COUNT(*) subquery, matching the dela.bg convention
            // for *Count properties (rather than relying on flattening to the ICollection.Count
            // property, whose EF translation is less predictable).
            configuration.NewConfig<Source, SourceViewModel>()
                .Map(dest => dest.NewsCount, src => src.News.Count());
        }
    }
}
