namespace PressCenters.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PressCenters.Data.Models;

    public class MainNewsSourcesSeeder : ISeeder
    {
        public void Seed(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var mainNewsSources = new List<(string Name, string Url, string TypeName)>
                                  {
                                      ("bTV", "https://btvnovinite.bg",
                                          "PressCenters.Services.Sources.MainNews.BtvNoviniteMainNewsProvider"),
                                      ("Nova", "https://nova.bg",
                                          "PressCenters.Services.Sources.MainNews.NovaBgMainNewsProvider"),
                                      ("News.bg", "https://news.bg",
                                          "PressCenters.Services.Sources.MainNews.NewsBgMainNewsProvider"),
                                      ("Dnes.bg", "https://www.dnes.bg",
                                          "PressCenters.Services.Sources.MainNews.DnesBgMainNewsProvider"),
                                      ("Novini.bg", "https://novini.bg",
                                          "PressCenters.Services.Sources.MainNews.NoviniBgMainNewsProvider"),
                                      ("Vesti.bg", "https://www.vesti.bg",
                                          "PressCenters.Services.Sources.MainNews.VestiBgMainNewsProvider"),
                                  };

            foreach (var mainNewsSource in mainNewsSources)
            {
                if (!dbContext.MainNewsSources.Any(x => x.TypeName == mainNewsSource.TypeName))
                {
                    dbContext.MainNewsSources.Add(
                        new MainNewsSource
                        {
                            TypeName = mainNewsSource.TypeName,
                            Name = mainNewsSource.Name,
                            Url = mainNewsSource.Url,
                        });
                }
            }
        }
    }
}
