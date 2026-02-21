using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface IApplicationRepository : IRepository<Application>
    {
        Task<IEnumerable<Application>> GetApplicationsByUserAsync(string userId);
        Task<IEnumerable<Application>> GetApplicationsByVacancyAsync(int vacancyId);
        Task<Application?> GetApplicationWithDetailsAsync(int id);
        Task<bool> UpdateStatusAsync(int id, ApplicationStatus status, string? employerComment = null);
        Task<bool> HasUserAppliedToVacancyAsync(string userId, int vacancyId);
        Task<int> GetApplicationsCountByVacancyAsync(int vacancyId);
        Task<int> GetApplicationsCountByUserAsync(string userId);
        Task<IEnumerable<Application>> GetApplicationsByCompanyAsync(int companyId);
    }
}
