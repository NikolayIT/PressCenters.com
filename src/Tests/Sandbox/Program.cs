namespace Sandbox
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using CommandLine;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using PressCenters.Data;
    using PressCenters.Data.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Data.Repositories;
    using PressCenters.Data.Seeding;
    using PressCenters.Services.Data;
    using PressCenters.Services.Messaging;
    using PressCenters.Services.Sources.BgInstitutions;

    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine($"{typeof(Program).Namespace} ({string.Join(" ", args)}) starts working...");
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(true);

            // Seed data on application startup
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                ApplicationDbContextSeeder.Seed(dbContext, serviceScope.ServiceProvider);
            }

            using (var serviceScope = serviceProvider.CreateScope())
            {
                serviceProvider = serviceScope.ServiceProvider;

                return Parser.Default.ParseArguments<SandboxOptions>(args).MapResult(
                    (SandboxOptions opts) => SandboxCode(opts, serviceProvider),
                    _ => 255);
            }
        }

        private static int SandboxCode(SandboxOptions options, IServiceProvider serviceProvider)
        {
            var sw = Stopwatch.StartNew();

            var newsRepository = serviceProvider.GetService<IDeletableEntityRepository<News>>();
            var sourcesRepository = serviceProvider.GetService<IDeletableEntityRepository<Source>>();
            var source = sourcesRepository.AllWithDeleted().FirstOrDefault(x => x.TypeName == "PressCenters.Services.Sources.BgInstitutions.MvrBgSource");
            var sourceProvider = new MvrBgSource();
            Console.WriteLine("Starting GetAllPublications...");
            var news = sourceProvider.GetAllPublications();
            Console.WriteLine("GetAllPublications done.");
            foreach (var remoteNews in news)
            {
                if (newsRepository.AllWithDeleted().Any(x => x.SourceId == source.Id && x.RemoteId == remoteNews.RemoteId))
                {
                    // Already exists
                    continue;
                }

                var dbNews = new News
                           {
                               Title = remoteNews.Title,
                               OriginalUrl = remoteNews.OriginalUrl,
                               ImageUrl = remoteNews.ImageUrl,
                               Content = remoteNews.Content,
                               CreatedOn = remoteNews.PostDate,
                               Source = source,
                               RemoteId = remoteNews.RemoteId,
                           };
                newsRepository.AddAsync(dbNews).GetAwaiter().GetResult();
            }

            newsRepository.SaveChangesAsync().GetAwaiter().GetResult();

            Console.WriteLine(sw.Elapsed);
            return 0;
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                    .UseLoggerFactory(new LoggerFactory()));

            services
                .AddIdentity<ApplicationUser, ApplicationRole>(IdentityOptionsProvider.GetIdentityOptions)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserStore<ApplicationUserStore>()
                .AddRoleStore<ApplicationRoleStore>()
                .AddDefaultTokenProviders();

            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // Application services
            services.AddTransient<IEmailSender, NullMessageSender>();
            services.AddTransient<ISmsSender, NullMessageSender>();
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddScoped<IWorkerTasksDataService, WorkerTasksDataService>();
        }
    }
}
