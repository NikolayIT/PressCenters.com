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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

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
    using PressCenters.Services.Mapping;
    using PressCenters.Services.Messaging;
    using PressCenters.Web.ViewModels;

    using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            GlobalConstants.SystemVersion =
                $"v2.0.{new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTime:yyyyMMdd}";

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
                                UsePageLocksOnDequeue = true,
                                DisableGlobalLocks = true,
                            }).UseConsole());

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(this.configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddApplicationInsightsTelemetry();

            services.AddSingleton(this.configuration);

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

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
            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);

            // Seed data on application startup
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                ApplicationDbContextSeeder.Seed(dbContext, serviceScope.ServiceProvider);
                this.SeedHangfireJobs(recurringJobManager, dbContext);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(
                new StaticFileOptions
                    {
                        OnPrepareResponse = ctx =>
                            {
                                if (ctx.Context.Request.Path.ToString().Contains("/news/"))
                                {
                                    // Cache static files for 90 days
                                    ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=31536000");
                                    ctx.Context.Response.Headers.Add(
                                        "Expires",
                                        DateTime.UtcNow.AddYears(1).ToString("R", CultureInfo.InvariantCulture));
                                }
                            },
                    });

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            if (env.IsProduction())
            {
                app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = 2 });
                app.UseHangfireDashboard(
                    "/hangfire",
                    new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } });
            }

            app.UseEndpoints(endpoints =>
            {
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

        private void SeedHangfireJobs(IRecurringJobManager recurringJobManager, ApplicationDbContext dbContext)
        {
            recurringJobManager.AddOrUpdate<DbCleanupJob>("DbCleanupJob", x => x.Work(), Cron.Weekly);
            recurringJobManager.AddOrUpdate<MainNewsGetterJob>("MainNewsGetterJob", x => x.Work(null), "*/2 * * * *");
            var sources = dbContext.Sources.Where(x => !x.IsDeleted).ToList();
            foreach (var source in sources)
            {
                recurringJobManager.AddOrUpdate<GetLatestPublicationsJob>(
                    $"GetLatestPublicationsJob_{source.Id}_{source.ShortName}",
                    x => x.Work(source.TypeName, null),
                    "*/5 * * * *");
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
