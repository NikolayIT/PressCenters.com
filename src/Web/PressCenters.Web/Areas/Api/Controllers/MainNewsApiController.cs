namespace PressCenters.Web.Areas.Api.Controllers
{
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data;
    using PressCenters.Web.Areas.Api.Models;

    [Route("api/main-news")]
    public class MainNewsApiController : ApiController
    {
        private readonly IDeletableEntityRepository<MainNews> mainNewsRepository;

        private readonly IMainNewsSourcesService mainNewsSourcesService;

        private readonly IDeletableEntityRepository<ApplicationUser> usersRepository;

        public MainNewsApiController(
            IDeletableEntityRepository<MainNews> mainNewsRepository,
            IMainNewsSourcesService mainNewsSourcesService,
            IDeletableEntityRepository<ApplicationUser> usersRepository)
        {
            this.mainNewsRepository = mainNewsRepository;
            this.mainNewsSourcesService = mainNewsSourcesService;
            this.usersRepository = usersRepository;
        }

        // GET /api/main-news -- the latest "top story" per source. Auth: the caller's X-Api-Key request header.
        [HttpGet]
        public IActionResult Get()
        {
            var apiKey = this.Request.Headers["X-Api-Key"].ToString();
            if (string.IsNullOrWhiteSpace(apiKey)
                || !this.usersRepository.AllAsNoTracking().Any(u => u.ApiKey == apiKey))
            {
                return this.Unauthorized(new { error = "Invalid or missing X-Api-Key header." });
            }

            var latestIds = this.mainNewsSourcesService.GetLatestMainNewsIds();
            var latest = this.mainNewsRepository.AllAsNoTracking()
                .Where(m => latestIds.Contains(m.Id))
                .OrderByDescending(m => m.CreatedOn)
                .Select(m => new { m.SourceId, SourceName = m.Source.Name, m.Title, m.OriginalUrl, m.CreatedOn })
                .ToList();

            var result = latest.Select(x => new MainNewsApiModel
            {
                Type = x.SourceName,
                Title = x.Title,
                ImageUrl = $"{GlobalConstants.SystemBaseUrl}/images/mainnews/{x.SourceId}.png",
                OriginalUrl = x.OriginalUrl,
                Time = x.CreatedOn,
            });

            return this.Ok(result);
        }
    }
}
