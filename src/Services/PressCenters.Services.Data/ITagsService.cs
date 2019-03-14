namespace PressCenters.Services.Data
{
    using System.Threading.Tasks;

    public interface ITagsService
    {
        Task UpdateTagsAsync(int id, string content);
    }
}
