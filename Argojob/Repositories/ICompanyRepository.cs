using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<Company?> GetCompanyWithVacanciesAsync(int id);
        Task<IEnumerable<Company>> GetVerifiedCompaniesAsync();
        Task<Company?> GetCompanyByNameAsync(string name);
        Task<int> GetVacanciesCountAsync(int companyId);
        Task<IEnumerable<Company>> SearchCompaniesAsync(string searchTerm);

        // Новые методы для работы с пользователями
        Task<IEnumerable<Company>> GetCompaniesByUserAsync(string userId);
        Task<bool> IsUserInCompanyAsync(string userId, int companyId);
        Task<string?> GetUserRoleInCompanyAsync(string userId, int companyId);
        Task<bool> AddUserToCompanyAsync(string userId, int companyId, string? role = null);
        Task<bool> RemoveUserFromCompanyAsync(string userId, int companyId);
        Task<bool> UpdateUserRoleAsync(string userId, int companyId, string newRole);

        Task<IEnumerable<CompanyUser>> GetCompanyUsersWithDetailsAsync(int companyId);
    }
}