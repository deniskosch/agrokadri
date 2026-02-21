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
                .Include(c => c.CompanyUsers)
                    .ThenInclude(cu => cu.User)
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

        // Новые методы для работы со связью пользователей
        public async Task<IEnumerable<Company>> GetCompaniesByUserAsync(string userId)
        {
            return await _context.CompanyUsers
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Company)
                .Select(cu => cu.Company)
                .ToListAsync();
        }

        public async Task<bool> IsUserInCompanyAsync(string userId, int companyId)
        {
            return await _context.CompanyUsers
                .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId);
        }

        public async Task<string?> GetUserRoleInCompanyAsync(string userId, int companyId)
        {
            var companyUser = await _context.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CompanyId == companyId);

            return companyUser?.Role;
        }

        public async Task<bool> AddUserToCompanyAsync(string userId, int companyId, string? role = null)
        {
            // Проверяем, не существует ли уже такая связь
            var exists = await IsUserInCompanyAsync(userId, companyId);
            if (exists)
                return false;

            var companyUser = new CompanyUser
            {
                UserId = userId,
                CompanyId = companyId,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            await _context.CompanyUsers.AddAsync(companyUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserFromCompanyAsync(string userId, int companyId)
        {
            var companyUser = await _context.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CompanyId == companyId);

            if (companyUser == null)
                return false;

            _context.CompanyUsers.Remove(companyUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, int companyId, string newRole)
        {
            var companyUser = await _context.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CompanyId == companyId);

            if (companyUser == null)
                return false;

            companyUser.Role = newRole;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CompanyUser>> GetCompanyUsersWithDetailsAsync(int companyId)
        {
            return await _context.CompanyUsers
                .Include(cu => cu.User)
                .Where(cu => cu.CompanyId == companyId)
                .ToListAsync();
        }
    }
}