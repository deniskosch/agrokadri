using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithVacanciesAsync(int id);
        Task<Category?> GetCategoryByNameAsync(string name);
        Task<IEnumerable<Category>> GetCategoriesWithCountsAsync();
        Task<int> GetVacanciesCountAsync(int categoryId);
    }
}
