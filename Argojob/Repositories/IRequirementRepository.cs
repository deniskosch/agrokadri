using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface IRequirementRepository : IRepository<Requirement>
    {
        Task<IEnumerable<Requirement>> GetRequirementsByVacancyAsync(int vacancyId);
        Task<bool> DeleteAllByVacancyAsync(int vacancyId);
        Task<List<int>> AddRangeToVacancyAsync(int vacancyId, List<string> requirementTexts);
    }
}
