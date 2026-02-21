using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Company?> GetCompanyWithVacanciesAsync(int id)
        {
            return await _context.Companies
                .Include(c => c.Vacancies.Where(v => v.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Company>> GetVerifiedCompaniesAsync()
        {
            return await _context.Companies
                .Where(c => c.IsVerified)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Company?> GetCompanyByNameAsync(string name)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<int> GetVacanciesCountAsync(int companyId)
        {
            return await _context.Vacancies
                .CountAsync(v => v.CompanyId == companyId && v.IsActive);
        }

        public async Task<IEnumerable<Company>> SearchCompaniesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            searchTerm = searchTerm.ToLower();

            return await _context.Companies
                .Where(c => c.Name.ToLower().Contains(searchTerm) ||
                           (c.Description != null && c.Description.ToLower().Contains(searchTerm)))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
