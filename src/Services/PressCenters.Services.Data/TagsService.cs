namespace PressCenters.Services.Data
{
    using System.Linq;
    using System.Threading.Tasks;

    using PressCenters.Data.Common.Repositories;
    using PressCenters.Data.Models;

    public class TagsService : ITagsService
    {
        private static readonly string[] CommonTags =
        {
            "Благоевград", "Бургас", "Варна", "Велико Търново", "Видин", "Враца ", "Габрово", "Добрич", "Кърджали",
            "Кюстендил", "Ловеч", "Монтана", "Пазарджик", "Плевен", "Перник", "Пловдив", "Разград", "Русе",
            "Силистра", "Сливен", "Смолян", "София", "Стара Загора", "Търговище", "Хасково", "Шумен", "Ямбол",
        };

        private readonly IRepository<Tag> tagsRepository;

        private readonly IDeletableEntityRepository<NewsTag> newsTagsRepository;

        public TagsService(IRepository<Tag> tagsRepository, IDeletableEntityRepository<NewsTag> newsTagsRepository)
        {
            this.tagsRepository = tagsRepository;
            this.newsTagsRepository = newsTagsRepository;
        }

        public async Task UpdateTagsAsync(int id, string content)
        {
            foreach (var commonTag in CommonTags)
            {
                if (content.Contains(commonTag))
                {
                    var tagId = await this.GetTagId(commonTag);
                    if (!this.newsTagsRepository.AllWithDeleted().Any(x => x.NewsId == id && x.TagId == tagId))
                    {
                        var newsTag = new NewsTag { NewsId = id, TagId = tagId };
                        await this.newsTagsRepository.AddAsync(newsTag);
                    }
                }
            }

            await this.newsTagsRepository.SaveChangesAsync();
        }

        private async Task<int> GetTagId(string commonTag)
        {
            var tagId = this.tagsRepository.All().Where(x => x.Name == commonTag).Select(x => x.Id).FirstOrDefault();
            if (tagId == 0)
            {
                var tag = new Tag { Name = commonTag };
                await this.tagsRepository.AddAsync(tag);
                await this.tagsRepository.SaveChangesAsync();
                tagId = tag.Id;
            }

            return tagId;
        }
    }
}
