using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetCategoryWithVacanciesAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Vacancies.Where(v => v.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithCountsAsync()
        {
            return await _context.Categories
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Vacancies = c.Vacancies.Where(v => v.IsActive).ToList()
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<int> GetVacanciesCountAsync(int categoryId)
        {
            return await _context.Vacancies
                .CountAsync(v => v.CategoryId == categoryId && v.IsActive);
        }
    }
}
