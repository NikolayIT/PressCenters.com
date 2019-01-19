namespace PressCenters.Worker.Runner
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using PressCenters.Data;
    using PressCenters.Data.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Data.Repositories;
    using PressCenters.Data.Seeding;
    using PressCenters.Services.Data;
    using PressCenters.Worker.Common;
    using PressCenters.Worker.Tasks;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
#if DEBUG
            isService = false;
#endif

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }
            else
            {
                Console.OutputEncoding = Encoding.UTF8;
            }

            var builder = new HostBuilder().ConfigureLogging(
                x =>
                {
                    x.ClearProviders();
                    x.AddProvider(new OneLineConsoleLoggerProvider(!isService));
                }).ConfigureServices(ConfigureServices);

            if (isService)
            {
                await builder.RunAsServiceAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables().Build();
            services.AddSingleton<IConfiguration>(configuration);

            var loggerFactory = new LoggerFactory();
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                    .UseLoggerFactory(loggerFactory));

            services.AddIdentity<ApplicationUser, ApplicationRole>(IdentityOptionsProvider.GetIdentityOptions)
                .AddEntityFrameworkStores<ApplicationDbContext>().AddUserStore<ApplicationUserStore>()
                .AddRoleStore<ApplicationRoleStore>().AddDefaultTokenProviders();

            // Identity stores
            services.AddTransient<IUserStore<ApplicationUser>, ApplicationUserStore>();
            services.AddTransient<IRoleStore<ApplicationRole>, ApplicationRoleStore>();

            // Seed data on application startup
            using (var serviceScope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                ApplicationDbContextSeeder.Seed(dbContext, serviceScope.ServiceProvider);
            }

            // Data services
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // Application services
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddTransient<INewsService, NewsService>();
            services.AddTransient<IWorkerTasksDataService, WorkerTasksDataService>();

            // Register TaskRunnerHostedService
            services.AddTransient<ITasksAssemblyProvider, TasksAssemblyProvider>();
            var parallelTasksCount = int.Parse(configuration["TasksExecutor:ParallelTasksCount"]);
            for (var i = 0; i < parallelTasksCount; i++)
            {
                services.AddHostedService<TasksExecutor>();
            }
        }

        public class TasksAssemblyProvider : ITasksAssemblyProvider
        {
            public Assembly GetAssembly()
            {
                return typeof(DbCleanupTask).Assembly;
            }
        }
    }
}
