namespace Sandbox.Code
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Services.Sources;

    using SixLabors.ImageSharp;

    public class DownloadImagesSandbox
    {
        public async Task Work(IServiceProvider serviceProvider)
        {
            const string RootPath = @"C:\Web\presscenters.com\wwwroot";
            var newsService = serviceProvider.GetService<INewsService>();
            var newsRepository = serviceProvider.GetService<IDeletableEntityRepository<News>>();
            var allNews = newsRepository.AllWithDeleted().Count();
            for (var i = 0; i <= allNews / 1000; i++)
            {
                Console.WriteLine($"{i}/{allNews / 1000}");
                var news = newsRepository.AllWithDeleted().OrderByDescending(x => x.Id)
                    .Select(x => new { x.Id, x.ImageUrl, x.Source.TypeName }).Skip(i * 1000).Take(1000);
                foreach (var newsItem in news)
                {
                    if (newsItem.ImageUrl == null || File.Exists(
                            RootPath + $"/images/news/{newsItem.Id % 1000}/big_{newsItem.Id}.png"))
                    {
                        continue;
                    }

                    var useProxy = false;
                    if (newsItem.TypeName != null)
                    {
                        var source = ReflectionHelpers.GetInstance<BaseSource>(newsItem.TypeName);
                        useProxy = source.UseProxy;
                    }

                    try
                    {
                        var result = await newsService.SaveImageLocallyAsync(
                                         newsItem.ImageUrl,
                                         newsItem.Id,
                                         RootPath,
                                         useProxy);
                        Console.Write(result ? "." : $"_{newsItem.Id}_");
                    }
                    catch (UnknownImageFormatException)
                    {
                        Console.Write("?");
                    }
                    catch (ImageFormatException)
                    {
                        Console.Write("!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"_{newsItem.Id}_");
                        Console.WriteLine(e);
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
