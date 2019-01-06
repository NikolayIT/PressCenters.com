namespace PressCenters.Web.Controllers
{
    using System;
    using System.Text;

    using Microsoft.AspNetCore.Mvc;

    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return this.RedirectToAction("List", "News");
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => this.View();

        [Route("robots.txt", Name = "GetRobotsText")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        public IActionResult RobotsTxt() =>
            this.Content("User-agent: *" + Environment.NewLine + "Disallow:", "text/plain", Encoding.UTF8);
    }
}
