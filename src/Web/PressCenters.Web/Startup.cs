namespace PressCenters.Web
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Hangfire;
    using Hangfire.Console;
    using Hangfire.Dashboard;
    using Hangfire.SqlServer;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using OpenTelemetry.Metrics;

    using PressCenters.Common;
    using PressCenters.Data;
    using PressCenters.Data.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Data.Repositories;
    using PressCenters.Data.Seeding;
    using PressCenters.Services;
    using PressCenters.Services.CronJobs;
    using PressCenters.Services.Data;
    using PressCenters.Services.Data.Images;
    using PressCenters.Services.Mapping;
    using PressCenters.Services.Messaging;
    using PressCenters.Web.ViewModels;

    using WebOptimizer;

    using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.configuration = configuration;
            this.env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            GlobalConstants.SystemVersion =
                $"v2.0.{new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTime:yyyyMMdd}";

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(this.configuration.GetConnectionString("DefaultConnection"))
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddHangfire(
                config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings().UseSqlServerStorage(
                        this.configuration.GetConnectionString("DefaultConnection"),
                        new SqlServerStorageOptions
                        {
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero,
                            UseRecommendedIsolationLevel = true,
                            DisableGlobalLocks = true,
                        }).UseConsole());

            if (this.env.IsProduction())
            {
                services.AddHangfireServer(options => options.WorkerCount = 2);
            }

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<CookiePolicyOptions>(
                options =>
                {
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

            // The cookie policy drops non-essential cookies until the visitor accepts the cookie banner, and the
            // TempData cookie is non-essential by default. That broke the contact form for those visitors (the
            // redirect to ThankYou found no TempData and returned 404 after the message was already saved) and
            // hid the Identity status messages. TempData is a functional cookie, not tracking.
            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.IsEssential = true);

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddWebOptimizer(
                pipeline =>
                {
                    pipeline.AddCssBundle("/css/site.min.css", "css/site.css");
                    pipeline.AddCssBundle("/css/unify.min.css", "css/unify.css");
                    pipeline.AddJavaScriptBundle("/js/site.min.js", "js/site.js");
                },
                options => options.EnableTagHelperBundling = true);
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Application Insights uses the OpenTelemetry-based SDK (v3). Cost control:
            // (1) sample traces + dependencies to 5% - request/dependency/failure rates
            //     stay accurate because the ApplicationInsightsSampler records itemCount.
            //     Was 20% until September 2026; the Hangfire jobs' "SQL: update" dependencies
            //     (~450k/week) still made this app the bulk of the shared-logs-weu workspace;
            // (2) drop the high-volume, low-value HttpClient connection-pool metrics that
            //     the Hangfire scraping jobs generate (http.client.open_connections alone
            //     was ~2.3M data points/week). Latency histograms are kept.
            services.AddApplicationInsightsTelemetry(options => options.SamplingRatio = 0.05f);
            services.AddOpenTelemetry().WithMetrics(metrics => metrics
                .AddView("http.client.open_connections", MetricStreamConfiguration.Drop)
                .AddView("http.client.active_requests", MetricStreamConfiguration.Drop)
                .AddView("http.client.connection.duration", MetricStreamConfiguration.Drop)
                .AddView("http.client.request.time_in_queue", MetricStreamConfiguration.Drop));

            services.AddSingleton(this.configuration);

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // News thumbnails: Cloudflare R2 when the "R2" section is configured, wwwroot/images otherwise.
            GlobalConstants.ImagesBaseUrl = services.AddImageStorage(
                this.configuration,
                Path.Combine(this.env.WebRootPath, "images"));

            // Application services
            services.AddTransient<IEmailSender>(
                serviceProvider => new SendGridEmailSender(this.configuration["SendGrid:ApiKey"]));
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddTransient<INewsService, NewsService>();
            services.AddTransient<ISourcesService, SourcesService>();
            services.AddTransient<IMainNewsSourcesService, MainNewsSourcesService>();
            services.AddTransient<ISlugGenerator, SlugGenerator>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IRecurringJobManager recurringJobManager)
        {
            MappingConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);

            // Seed data on application startup
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                ApplicationDbContextSeeder.Seed(dbContext, serviceScope.ServiceProvider);
                SeedHangfireJobs(recurringJobManager, dbContext);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Until September 2026 the thumbnails were served from wwwroot as /images/news/{id % 1000}/{big|small}_{id}.png.
            // They now live on Cloudflare R2 behind GlobalConstants.ImagesBaseUrl, so anyone still holding an old URL
            // (search engines, social previews, hot-links) is sent to the same key on the new host; for a thumbnail, to its
            // WebP, as the PNGs are deleted once converted (October 2026). Only active when the base URL is an absolute
            // host; with the local wwwroot fallback the static files handler serves them.
            if (Uri.IsWellFormedUriString(GlobalConstants.ImagesBaseUrl, UriKind.Absolute))
            {
                app.UseRewriter(
                    new RewriteOptions()
                        .AddRedirect(
                            @"(?i)^images/news/(\d+/(?:big|small)_\d+)\.png$",
                            $"{GlobalConstants.ImagesBaseUrl}/news/$1.webp",
                            StatusCodes.Status301MovedPermanently)
                        .AddRedirect(
                            "(?i)^images/news/(.+)$",
                            $"{GlobalConstants.ImagesBaseUrl}/news/$1",
                            StatusCodes.Status301MovedPermanently));
            }

            app.UseWebOptimizer();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            if (env.IsProduction())
            {
                app.UseHangfireDashboard(
                    "/hangfire",
                    new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    "areaRoute",
                    "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    "news",
                    "News/{id:int:min(1)}/{slug:required}",
                    new { controller = "News", action = "ById", });
                endpoints.MapControllerRoute(
                    "news",
                    "News/{id:int:min(1)}",
                    new { controller = "News", action = "ById", });
                endpoints.MapControllerRoute("default", "{controller=News}/{action=List}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private static void SeedHangfireJobs(IRecurringJobManager recurringJobManager, ApplicationDbContext dbContext)
        {
            recurringJobManager.AddOrUpdate<DbCleanupJob>("DbCleanupJob", x => x.Work(), Cron.Weekly);
            recurringJobManager.AddOrUpdate<MainNewsGetterJob>("MainNewsGetterJob", x => x.Work(null), "*/2 * * * *");
            var sources = dbContext.Sources.Where(x => !x.IsDeleted).ToList();
            foreach (var source in sources)
            {
                recurringJobManager.AddOrUpdate<GetLatestPublicationsJob>(
                    $"GetLatestPublicationsJob_{source.Id}_{source.ShortName}",
                    x => x.Work(source.TypeName, null),
                    "*/10 * * * *");
            }
        }

        private class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                var httpContext = context.GetHttpContext();
                return httpContext.User.IsInRole(GlobalConstants.AdministratorRoleName);
            }
        }
    }
}
