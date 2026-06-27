namespace PressCenters.Web.Areas.Api.Controllers
{
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Web.Areas.Api.Models;

    [Route("api/main-news")]
    public class MainNewsApiController : ApiController
    {
        private readonly IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository;

        private readonly IDeletableEntityRepository<ApplicationUser> usersRepository;

        public MainNewsApiController(
            IDeletableEntityRepository<MainNewsSource> mainNewsSourcesRepository,
            IDeletableEntityRepository<ApplicationUser> usersRepository)
        {
            this.mainNewsSourcesRepository = mainNewsSourcesRepository;
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

            var sources = this.mainNewsSourcesRepository.AllAsNoTracking()
                .Select(s => new { s.Id, s.Name, Latest = s.MainNews.OrderByDescending(m => m.Id).FirstOrDefault() })
                .Where(x => x.Latest != null)
                .OrderByDescending(x => x.Latest.CreatedOn)
                .ToList();

            var result = sources.Select(x => new MainNewsApiModel
            {
                Type = x.Name,
                Title = x.Latest.Title,
                ImageUrl = $"{GlobalConstants.SystemBaseUrl}/images/mainnews/{x.Id}.png",
                OriginalUrl = x.Latest.OriginalUrl,
                Time = x.Latest.CreatedOn,
            });

            return this.Ok(result);
        }
    }
}
