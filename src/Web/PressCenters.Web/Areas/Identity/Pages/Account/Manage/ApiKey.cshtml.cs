namespace PressCenters.Web.Areas.Identity.Pages.Account.Manage
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    using PressCenters.Data.Models;

#pragma warning disable SA1649 // File name should match first type name
    public class ApiKeyModel : PageModel
#pragma warning restore SA1649 // File name should match first type name
    {
        private readonly UserManager<ApplicationUser> userManager;

        public ApiKeyModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public string ApiKey { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this.userManager.GetUserId(this.User)}'.");
            }

            // Self-heal in case a user somehow has no key yet (e.g. created before the column existed).
            if (string.IsNullOrEmpty(user.ApiKey))
            {
                user.ApiKey = ApplicationUser.GenerateApiKey();
                await this.userManager.UpdateAsync(user);
            }

            this.ApiKey = user.ApiKey;
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this.userManager.GetUserId(this.User)}'.");
            }

            user.ApiKey = ApplicationUser.GenerateApiKey();
            await this.userManager.UpdateAsync(user);

            this.StatusMessage = "Your API key was regenerated. Update it everywhere you use it.";
            return this.RedirectToPage();
        }
    }
}
