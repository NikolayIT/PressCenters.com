namespace Sandbox
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CommandLine;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using PressCenters.Common;
    using PressCenters.Data;
    using PressCenters.Data.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Data.Repositories;
    using PressCenters.Data.Seeding;
    using PressCenters.Services.Data;
    using PressCenters.Services.Messaging;
    using PressCenters.Services.Sources;
    using PressCenters.Worker.Common;
    using PressCenters.Worker.Tasks;

    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
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

                return Parser.Default.ParseArguments<SandboxOptions, RunTaskOptions>(args).MapResult(
                    (SandboxOptions opts) => SandboxCode(opts, serviceProvider),
                    (RunTaskOptions opts) => RunTask(opts, serviceProvider),
                    _ => 255);
            }
        }

        private static int SandboxCode(SandboxOptions options, IServiceProvider serviceProvider)
        {
            var sw = Stopwatch.StartNew();

            var newsService = serviceProvider.GetService<INewsService>();
            var tagsService = serviceProvider.GetService<ITagsService>();
            var sourcesRepository = serviceProvider.GetService<IDeletableEntityRepository<Source>>();
            foreach (var source in sourcesRepository.All().ToList())
            {
                // Run only for selected sources
                if (!new[] { "CiafGovernmentBgSource" }.Any(x => source.TypeName.Contains(x)))
                {
                    continue;
                }

                var sourceProvider = ReflectionHelpers.GetInstance<BaseSource>(source.TypeName);
                Console.WriteLine($"Starting {source.TypeName}.GetAllPublications...");
                var news = sourceProvider.GetAllPublications();
                foreach (var remoteNews in news)
                {
                    newsService.AddAsync(remoteNews, source.Id).GetAwaiter().GetResult();
                }

                Console.WriteLine($"{source.TypeName}.GetAllPublications done.");
            }

            Console.WriteLine(sw.Elapsed);
            return 0;
        }

        private static int RunTask(RunTaskOptions options, IServiceProvider serviceProvider)
        {
            try
            {
                var typeName = $"PressCenters.Worker.Tasks.{options.TaskName}";
                var assembly = typeof(DbCleanupTask).Assembly;
                var type = assembly.GetType(typeName);
                var constructor = type.GetConstructors()[0];
                var args = constructor.GetParameters().Select(p => serviceProvider.GetService(p.ParameterType)).ToArray();
                if (!(Activator.CreateInstance(type, args) is ITask task))
                {
                    Console.WriteLine($"Unable to create instance of \"{typeName}\"!");
                    return 1;
                }

                var sw = Stopwatch.StartNew();
                var result = task.DoWork(options.Parameters).GetAwaiter().GetResult();
                Console.WriteLine($"Result: {result}");
                Console.WriteLine($"Time elapsed: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 2;
            }

            return 0;
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            var loggerFactory = new LoggerFactory();
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                    .EnableSensitiveDataLogging().UseLoggerFactory(loggerFactory));

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddSingleton<ILoggerFactory>(
                provider =>
                {
                    var factory = new LoggerFactory();
                    factory.AddProvider(new OneLineConsoleLoggerProvider(true));
                    return factory;
                });
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // Application services
            services.AddTransient<IEmailSender, NullMessageSender>();
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddTransient<INewsService, NewsService>();
            services.AddTransient<ITagsService, TagsService>();
            services.AddTransient<IWorkerTasksDataService, WorkerTasksDataService>();
        }
    }
}
