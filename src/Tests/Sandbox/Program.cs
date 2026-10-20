namespace Sandbox
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using CommandLine;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
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
    using PressCenters.Services.Data.Images;
    using PressCenters.Services.Messaging;

    using Sandbox.Code;

    using Spectre.Console;

    public static class Program
    {
        private const string DatabaseVariable = "SANDBOX_DATABASE";

        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine($"{typeof(Program).Namespace} ({string.Join(" ", args)}) starts working...");
            var configuration = BuildConfiguration();
            var connectionString = GetConnectionString(configuration);
            if (connectionString == null)
            {
                return 1;
            }

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration, connectionString);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(true);

            // Seed data on application startup. Skippable (BACKFILL_SKIP_MIGRATE=1) so a backfill run against an
            // already-migrated/seeded production database performs no schema or seed side effects -- only the
            // intended News inserts. The R2 upload and WebP conversion modes never touch the database, so they always
            // skip this.
            var webpMode = Environment.GetEnvironmentVariable(ConvertThumbnailsToWebpSandbox.ModeVariable) == "1";

            // A mode chosen with an environment variable (as the launch profiles do) wins over the R2:UploadRoot
            // switch, which stays in appsettings.Production.json after the one-time upload.
            var uploadImagesMode = !webpMode && Environment.GetEnvironmentVariable("IMAGES_BACKFILL") != "1"
                && !string.IsNullOrWhiteSpace(configuration[UploadImagesToR2Sandbox.UploadRootKey]);
            if (!uploadImagesMode && !webpMode && Environment.GetEnvironmentVariable("BACKFILL_SKIP_MIGRATE") != "1")
            {
                using var serviceScope = serviceProvider.CreateScope();
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                ApplicationDbContextSeeder.Seed(dbContext, serviceScope.ServiceProvider);
            }

            using (var serviceScope = serviceProvider.CreateScope())
            {
                serviceProvider = serviceScope.ServiceProvider;

                return Parser.Default.ParseArguments<SandboxOptions>(args).MapResult(
                    (SandboxOptions opts) => SandboxCode(opts, serviceProvider, uploadImagesMode),
                    _ => 255);
            }
        }

        private static int SandboxCode(SandboxOptions options, IServiceProvider serviceProvider, bool uploadImagesMode)
        {
            var sw = Stopwatch.StartNew();

            //// serviceProvider.GetService<INewsService>().SaveImageAsync("https://prb.bg/upload/55508/%D0%93%D0%B5%D1%80%D0%B1+%D0%92%D0%B8%D1%82%D1%80%D0%B0%D0%B6.JPG", 191333, false).GetAwaiter().GetResult();
            //// new UpdateSearchTextSandbox().Work(serviceProvider).GetAwaiter().GetResult();
            if (uploadImagesMode)
            {
                new UploadImagesToR2Sandbox().Work(serviceProvider).GetAwaiter().GetResult();
            }
            else if (Environment.GetEnvironmentVariable(ConvertThumbnailsToWebpSandbox.ModeVariable) == "1")
            {
                new ConvertThumbnailsToWebpSandbox().Work(serviceProvider).GetAwaiter().GetResult();
            }
            else if (Environment.GetEnvironmentVariable("IMAGES_BACKFILL") == "1")
            {
                new DownloadImagesSandbox().Work(serviceProvider).GetAwaiter().GetResult();
            }
            else
            {
                new GetAllNewsSandbox().Work(serviceProvider).GetAwaiter().GetResult();
            }

            Console.WriteLine(sw.Elapsed);
            return 0;
        }

        private static IConfiguration BuildConfiguration()
        {
            // appsettings.Production.json is git-ignored (the repo is public) and holds the secrets: the production
            // connection string (on a dev box under ConnectionStrings:Production, see GetConnectionString), the R2
            // credentials and the R2:UploadRoot switch. Copy it next to Sandbox.exe on the server.
            return new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.Production.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }

        // DefaultConnection: the local database on a dev box, the production one on the server. SANDBOX_DATABASE=<name>
        // (Production in the "(production database)" launch profiles) runs against ConnectionStrings:<name> instead:
        // from the git-ignored appsettings.Production.json or the ConnectionStrings__<name> environment variable, else
        // asked for in the console, so that the password never goes into launchSettings.json. Null: there is none.
        private static string GetConnectionString(IConfiguration configuration)
        {
            var name = Environment.GetEnvironmentVariable(DatabaseVariable);
            if (string.IsNullOrWhiteSpace(name))
            {
                return configuration.GetConnectionString("DefaultConnection");
            }

            var connectionString = configuration.GetConnectionString(name);
            if (string.IsNullOrWhiteSpace(connectionString) && AnsiConsole.Profile.Capabilities.Interactive)
            {
                connectionString = AnsiConsole.Prompt(
                    new TextPrompt<string>(
                            $"Connection string of the [yellow]{Markup.Escape(name)}[/] database"
                            + $" [grey](or put it in appsettings.Production.json as ConnectionStrings:{Markup.Escape(name)})[/]:")
                        .Secret());
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine(
                    $"{DatabaseVariable}={name}, but there is no connection string for it: put it in appsettings.Production.json"
                    + $" as ConnectionStrings:{name}, or in the ConnectionStrings__{name} environment variable.");
                return null;
            }

            return connectionString;
        }

        private static void ConfigureServices(ServiceCollection services, IConfiguration configuration, string connectionString)
        {
            services.AddSingleton<IConfiguration>(configuration);
            var loggerFactory = new LoggerFactory();
            services.AddDbContext<ApplicationDbContext>(
                options => options
                    .UseSqlServer(
                        connectionString,
                        sqlServerOptions => sqlServerOptions.CommandTimeout(600)).EnableSensitiveDataLogging()
                    .UseLoggerFactory(loggerFactory)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

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

            // News thumbnails: R2 when the "R2" section is filled in (appsettings.Production.json); otherwise a
            // local folder (Images:LocalRoot, default C:\Web\presscenters.com\wwwroot\images) in the pre-R2 layout.
            var localImagesRoot = configuration["Images:LocalRoot"] ?? @"C:\Web\presscenters.com\wwwroot\images";
            services.AddImageStorage(configuration, localImagesRoot);

            // Application services
            services.AddTransient<IEmailSender, NullMessageSender>();
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddTransient<INewsService, NewsService>();
            services.AddTransient<ITagsService, TagsService>();
        }
    }
}
