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
            // Env-driven so a backfill operator can stage thumbnails into a local folder (then copy to the prod
            // web root) and limit the scan to a recent id window instead of every article.
            var rootPath = Environment.GetEnvironmentVariable("IMAGES_WEBROOT") ?? @"C:\Web\presscenters.com\wwwroot";
            var minId = int.TryParse(Environment.GetEnvironmentVariable("IMAGES_MIN_ID"), out var mi) ? mi : 0;
            var sources = (Environment.GetEnvironmentVariable("IMAGES_SOURCES") ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var newsService = serviceProvider.GetService<INewsService>();
            var newsRepository = serviceProvider.GetService<IDeletableEntityRepository<News>>();
            var allNews = newsRepository.AllWithDeleted().Count();
            for (var i = 0; i <= allNews / 1000; i++)
            {
                Console.WriteLine($"{i}/{allNews / 1000}");
                var news = newsRepository.AllWithDeleted().OrderByDescending(x => x.Id)
                    .Select(x => new { x.Id, x.ImageUrl, x.Source.TypeName }).Skip(i * 1000).Take(1000).ToList();
                foreach (var newsItem in news)
                {
                    if (newsItem.Id < minId || newsItem.ImageUrl == null
                        || (sources.Length > 0 && (newsItem.TypeName == null || !sources.Any(t => newsItem.TypeName.Contains(t))))
                        || File.Exists(rootPath + $"/images/news/{newsItem.Id % 1000}/big_{newsItem.Id}.png"))
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
                                         rootPath,
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

                if (news.Count > 0 && news[news.Count - 1].Id < minId)
                {
                    break;
                }

                Console.WriteLine();
            }
        }
    }
}
