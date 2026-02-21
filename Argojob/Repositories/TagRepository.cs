using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
        {
            return await _context.Tags
                .OrderByDescending(t => t.VacancyTags.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsByVacancyAsync(int vacancyId)
        {
            return await _context.VacancyTags
                .Where(vt => vt.VacancyId == vacancyId)
                .Select(vt => vt.Tag)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetTagsWithCountsAsync()
        {
            return await _context.Tags
                .Select(t => new { t.Name, Count = t.VacancyTags.Count })
                .ToDictionaryAsync(t => t.Name, t => t.Count);
        }

        public async Task<List<int>> AddTagsToVacancyAsync(int vacancyId, List<string> tagNames)
        {
            var tagIds = new List<int>();
            var vacancy = await _context.Vacancies.FindAsync(vacancyId);

            if (vacancy == null)
                return tagIds;

            foreach (var tagName in tagNames.Distinct())
            {
                // Ищем существующий тег
                var tag = await GetTagByNameAsync(tagName);

                // Если тега нет, создаем новый
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    tag.Id = await AddAsync(tag);
                }

                // Проверяем, не привязан ли уже тег к вакансии
                var exists = await _context.VacancyTags
                    .AnyAsync(vt => vt.VacancyId == vacancyId && vt.TagId == tag.Id);

                if (!exists)
                {
                    _context.VacancyTags.Add(new VacancyTag
                    {
                        VacancyId = vacancyId,
                        TagId = tag.Id
                    });
                    tagIds.Add(tag.Id);
                }
            }

            await _context.SaveChangesAsync();
            return tagIds;
        }
    }
}
