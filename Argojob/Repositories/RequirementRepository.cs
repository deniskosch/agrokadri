using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    public class RequirementRepository : Repository<Requirement>, IRequirementRepository
    {
        public RequirementRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Requirement>> GetRequirementsByVacancyAsync(int vacancyId)
        {
            return await _context.Requirements
                .Where(r => r.VacancyId == vacancyId)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<bool> DeleteAllByVacancyAsync(int vacancyId)
        {
            var requirements = await GetRequirementsByVacancyAsync(vacancyId);
            if (!requirements.Any())
                return false;

            _context.Requirements.RemoveRange(requirements);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<int>> AddRangeToVacancyAsync(int vacancyId, List<string> requirementTexts)
        {
            var requirementIds = new List<int>();

            foreach (var text in requirementTexts.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var requirement = new Requirement
                {
                    VacancyId = vacancyId,
                    Text = text.Trim()
                };

                var id = await AddAsync(requirement);
                requirementIds.Add(id);
            }

            return requirementIds;
        }
    }

}
