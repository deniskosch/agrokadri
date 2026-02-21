using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
        Task<IEnumerable<Tag>> GetTagsByVacancyAsync(int vacancyId);
        Task<Dictionary<string, int>> GetTagsWithCountsAsync();
        Task<List<int>> AddTagsToVacancyAsync(int vacancyId, List<string> tagNames);
    }
}
