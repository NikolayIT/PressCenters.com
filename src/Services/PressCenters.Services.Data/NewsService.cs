namespace PressCenters.Services.Data
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using AngleSharp.Html.Parser;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;

    public class NewsService : INewsService
    {
        private readonly IDeletableEntityRepository<News> newsRepository;

        public NewsService(IDeletableEntityRepository<News> newsRepository)
        {
            this.newsRepository = newsRepository;
        }

        public async Task<int?> AddAsync(RemoteNews remoteNews, int sourceId)
        {
            if (this.newsRepository.AllWithDeleted()
                .Any(x => x.SourceId == sourceId && x.RemoteId == remoteNews.RemoteId))
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
                             RemoteId = remoteNews.RemoteId?.Trim(),
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

            // Remove all non-alphanumeric characters
            var regex = new Regex(@"[^\w\d]", RegexOptions.Compiled);
            text = regex.Replace(text, " ");

            // Split words and remove duplicate values
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length > 1).Distinct();

            // Combine all words
            return string.Join(" ", words);
        }

        public async Task<bool> SaveImageLocallyAsync(string imageUrl, int newsId, string webRoot, bool useProxy = false)
        {
            var directory = webRoot + $"/images/news/{newsId % 1000}/";
            Directory.CreateDirectory(directory);

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return false;
            }

            if (useProxy)
            {
                imageUrl = new Uri(imageUrl).GetLeftPart(UriPartial.Query); // Remove hash fragment
                imageUrl = imageUrl.Replace("https://", "https://proxy.presscenters.com/https/")
                        .Replace("http://", "https://proxy.presscenters.com/http/");
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(120), };
            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.122 Safari/537.36");
            var result = await client.GetAsync(imageUrl);
            if (!result.IsSuccessStatusCode)
            {
                return false;
            }

            var imageBytes = await result.Content.ReadAsByteArrayAsync();

            // Big thumbnail
            using var bigThumbnail = Image.Load(imageBytes);
            bigThumbnail.Mutate(
                x => x.Resize(
                    new ResizeOptions
                    {
                        Mode = ResizeMode.Min,
                        Size = new Size(730, 0),
                        Position = AnchorPositionMode.Center,
                    }));
            await SaveImage(bigThumbnail, "big_" + newsId);

            // Small thumbnail
            using var smallThumbnail = Image.Load(imageBytes);
            smallThumbnail.Mutate(
                x => x.Resize(
                    new ResizeOptions
                    {
                        Mode = ResizeMode.Crop,
                        Size = new Size(200, 120),
                        Position = AnchorPositionMode.Center,
                    }));
            await SaveImage(smallThumbnail, "small_" + newsId);
            return true;

            async Task SaveImage(Image image, string fileName)
            {
                var tempPath = directory + Path.GetRandomFileName() + ".png";
                await using (var stream = File.OpenWrite(tempPath))
                {
                    image.SaveAsPng(
                        stream,
                        new PngEncoder
                        {
                            FilterMethod = PngFilterMethod.Adaptive,
                            CompressionLevel = 9,
                            ColorType = PngColorType.Palette,
                        });
                }

                var filePath = directory + fileName + ".png";
                File.Delete(filePath);
                File.Move(tempPath, filePath);
            }
        }
    }
}
