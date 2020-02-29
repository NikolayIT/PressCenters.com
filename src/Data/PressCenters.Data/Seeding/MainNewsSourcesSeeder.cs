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
                                      ("БНТ", "https://news.bnt.bg",
                                          "PressCenters.Services.Sources.MainNews.NewsBntBgMainNewsProvider"),
                                      ("bTV", "https://btvnovinite.bg",
                                          "PressCenters.Services.Sources.MainNews.BtvNoviniteMainNewsProvider"),
                                      ("Nova", "https://nova.bg",
                                          "PressCenters.Services.Sources.MainNews.NovaBgMainNewsProvider"),
                                      /*("News.bg", "https://news.bg",
                                          "PressCenters.Services.Sources.MainNews.NewsBgMainNewsProvider"),*/
                                      ("Dnes.bg", "https://www.dnes.bg",
                                          "PressCenters.Services.Sources.MainNews.DnesBgMainNewsProvider"),
                                      /*("Novini.bg", "https://novini.bg",
                                          "PressCenters.Services.Sources.MainNews.NoviniBgMainNewsProvider"),*/
                                      /*("Vesti.bg", "https://www.vesti.bg",
                                          "PressCenters.Services.Sources.MainNews.VestiBgMainNewsProvider"),*/
                                      ("CNN", "https://edition.cnn.com",
                                          "PressCenters.Services.Sources.MainNews.CnnMainNewsProvider"),
                                      ("Dnevnik.bg", "https://www.dnevnik.bg",
                                          "PressCenters.Services.Sources.MainNews.DnevnikBgMainNewsProvider"),
                                      ("Euronews", "https://www.euronews.com",
                                          "PressCenters.Services.Sources.MainNews.EuronewsMainNewsProvider"),
                                      ("BTA.bg", "http://www.bta.bg/bg",
                                          "PressCenters.Services.Sources.MainNews.BtaBgMainNewsProvider"),
                                      ("Mediapool.bg", "https://www.mediapool.bg",
                                          "PressCenters.Services.Sources.MainNews.MediapoolBgMainNewsProvider"),
                                      ("AP", "https://www.apnews.com",
                                          "PressCenters.Services.Sources.MainNews.ApMainNewsProvider"),
                                      ("Reuters", "https://www.reuters.com",
                                          "PressCenters.Services.Sources.MainNews.ReutersMainNewsProvider"),
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
