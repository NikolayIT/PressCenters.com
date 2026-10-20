namespace PressCenters.Services.Data
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using AngleSharp.Html.Parser;
    using PressCenters.Common;
    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;
    using PressCenters.Services.Data.Images;

    public class NewsService : INewsService
    {
        private static readonly Regex NonAlphanumericRegex = new Regex(@"[^\w\d]", RegexOptions.Compiled);

        private readonly IDeletableEntityRepository<News> newsRepository;

        private readonly IImageStorage imageStorage;

        public NewsService(IDeletableEntityRepository<News> newsRepository, IImageStorage imageStorage)
        {
            this.newsRepository = newsRepository;
            this.imageStorage = imageStorage;
        }

        public async Task<int?> AddAsync(RemoteNews remoteNews, int sourceId)
        {
            // Look up the same (trimmed) id that is stored below, otherwise an id with surrounding whitespace
            // never matches and the news is inserted again on every run.
            var remoteId = remoteNews.RemoteId?.Trim();
            if (this.newsRepository.AllWithDeleted()
                .Any(x => x.SourceId == sourceId && x.RemoteId == remoteId))
            {
                // Already exists
                return null;
            }

            var news = new News
                         {
                             Title = remoteNews.Title?.Trim(),
                             OriginalUrl = remoteNews.OriginalUrl?.Trim(),
                             ImageUrl = remoteNews.ImageUrl?.Trim(),
                             Content = remoteNews.Content?.Trim(),
                             CreatedOn = remoteNews.PostDate,
                             SourceId = sourceId,
                             RemoteId = remoteId,
                         };
            news.SearchText = this.GetSearchText(news);

            await this.newsRepository.AddAsync(news);
            await this.newsRepository.SaveChangesAsync();
            return news.Id;
        }

        public async Task UpdateAsync(int id, RemoteNews remoteNews)
        {
            var news = this.newsRepository.AllWithDeleted().FirstOrDefault(x => x.Id == id);
            if (news == null)
            {
                return;
            }

            if (remoteNews == null)
            {
                return;
            }

            news.Title = remoteNews.Title;
            news.OriginalUrl = remoteNews.OriginalUrl;
            news.ImageUrl = remoteNews.ImageUrl;
            news.Content = remoteNews.Content;
            news.RemoteId = remoteNews.RemoteId;
            news.SearchText = this.GetSearchText(news);
            //// We should not update the PostDate here

            await this.newsRepository.SaveChangesAsync();
        }

        public int Count()
        {
            return this.newsRepository.All().Count();
        }

        public string GetSearchText(News news)
        {
            // Get only text from content
            var parser = new HtmlParser();
            var document = parser.ParseDocument($"<html><body>{news.Content}</body></html>");

            // Append title
            var text = news.Title + " " + document.Body.TextContent;
            text = text.ToLower();

            // Remove all non-alphanumeric characters (a static compiled instance: compiling a new Regex per call
            // costs far more than the replace itself)
            text = NonAlphanumericRegex.Replace(text, " ");

            // Split words and remove duplicate values
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length > 1).Distinct();

            // Combine all words
            return string.Join(" ", words);
        }

        public async Task<bool> SaveImageAsync(string imageUrl, int newsId, bool useProxy = false)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return false;
            }

            var directUrl = new Uri(imageUrl).GetLeftPart(UriPartial.Query); // Remove hash fragment

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(120), };

            // Prefer HTTP/2 like the page fetch, and fail over across the relays: some image hosts
            // (e.g. minfin.bg) block this egress directly but answer the Cloudflare relay over HTTP/2,
            // and the relays are not interchangeable.
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
            client.DefaultRequestHeaders.Add("User-Agent", GlobalConstants.DefaultUserAgent);

            HttpResponseMessage result = null;
            if (useProxy)
            {
                foreach (var host in GlobalConstants.ProxyHosts.OrderBy(_ => Guid.NewGuid()))
                {
                    try
                    {
                        var candidate = await client.GetAsync(ProxyUrlBuilder.WrapWith(directUrl, host).Replace("+", "%20"));
                        if (candidate.IsSuccessStatusCode)
                        {
                            result = candidate;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // Relay host unreachable -- try the next one.
                    }
                }
            }

            if (result == null || !result.IsSuccessStatusCode)
            {
                try
                {
                    result = await client.GetAsync(directUrl);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            if (!result.IsSuccessStatusCode)
            {
                return false;
            }

            var imageBytes = await result.Content.ReadAsByteArrayAsync();

            // Both thumbnails before uploading either, so an image that cannot be decoded leaves nothing behind.
            var bigThumbnail = ImageThumbnails.ShrinkToWidth(imageBytes, 730, ThumbnailFormat.Webp);
            var smallThumbnail = ImageThumbnails.CropToFill(imageBytes, 200, 120, ThumbnailFormat.Webp);
            await this.UploadImage(bigThumbnail, NewsImagePaths.BigKey(newsId));
            await this.UploadImage(smallThumbnail, NewsImagePaths.SmallKey(newsId));
            return true;
        }

        private async Task UploadImage(byte[] thumbnail, string key)
        {
            using var stream = new MemoryStream(thumbnail);
            await this.imageStorage.UploadAsync(key, stream, NewsImagePaths.ContentType);
        }
    }
}
