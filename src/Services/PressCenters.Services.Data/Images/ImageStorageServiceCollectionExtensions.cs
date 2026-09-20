namespace PressCenters.Services.Data.Images
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ImageStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="IImageStorage"/>: Cloudflare R2 when the "R2" section is filled in, otherwise
        /// <see cref="LocalImageStorage"/> rooted at <paramref name="localRootPath"/> (normally wwwroot/images).
        /// Returns the public base URL that thumbnail links should be built from.
        /// </summary>
        public static string AddImageStorage(this IServiceCollection services, IConfiguration configuration, string localRootPath)
        {
            var options = configuration.GetSection(R2Options.SectionName).Get<R2Options>() ?? new R2Options();
            if (options.IsConfigured)
            {
                services.AddSingleton<IImageStorage>(new R2ImageStorage(options));
                return options.PublicBaseUrl?.TrimEnd('/');
            }

            services.AddSingleton<IImageStorage>(new LocalImageStorage(localRootPath));
            return "/images";
        }
    }
}
