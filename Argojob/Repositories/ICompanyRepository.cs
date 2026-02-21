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
    }
}
