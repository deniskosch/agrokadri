using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface IVacancyRepository : IRepository<Vacancy>
    {
        // Специфические методы для вакансий
        Task<Vacancy?> GetVacancyWithDetailsAsync(int id);
        Task<IEnumerable<Vacancy>> GetActiveVacanciesAsync();
        Task<IEnumerable<Vacancy>> GetSeasonalVacanciesAsync();
        Task<IEnumerable<Vacancy>> GetVacanciesByCategoryAsync(int categoryId);
        Task<IEnumerable<Vacancy>> GetVacanciesByCompanyAsync(int companyId);
        Task<IEnumerable<Vacancy>> GetVacanciesByLocationAsync(int locationId);
        Task<IEnumerable<Vacancy>> SearchVacanciesAsync(string searchTerm);
        Task<IEnumerable<Vacancy>> GetLatestVacanciesAsync(int count);
        Task<Dictionary<string, int>> GetVacancyStatsAsync();
        Task<bool> PublishVacancyAsync(int id);
        Task<bool> ArchiveVacancyAsync(int id);
        Task<int> IncrementViewsAsync(int id);

        // Сложные фильтры
        Task<IEnumerable<Vacancy>> FilterVacanciesAsync(
            int? categoryId = null,
            int? locationId = null,
            int? companyId = null,
            bool? isSeasonal = null,
            decimal? minSalary = null,
            decimal? maxSalary = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 10);
    }
}
