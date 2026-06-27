namespace PressCenters.Data.Seeding
{
    using System;
    using System.Linq;

    using PressCenters.Data.Models;

    // Backfills an API key for any existing user that does not have one yet (new users get theirs in the
    // ApplicationUser constructor). Runs on every startup; it is a no-op once every user has a key.
    public class ApiKeysSeeder : ISeeder
    {
        public void Seed(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var usersWithoutApiKey = dbContext.Users
                .Where(u => u.ApiKey == null || u.ApiKey == string.Empty)
                .ToList();
            foreach (var user in usersWithoutApiKey)
            {
                user.ApiKey = ApplicationUser.GenerateApiKey();
            }
        }
    }
}
