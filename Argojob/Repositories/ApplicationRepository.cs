using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    public class ApplicationRepository : Repository<Application>, IApplicationRepository
    {
        public ApplicationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Application>> GetApplicationsByUserAsync(string userId)
        {
            return await _context.Applications
                .Where(a => a.UserId == userId)
                .Include(a => a.Vacancy)
                    .ThenInclude(v => v.Company)
                .Include(a => a.Resume)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Application>> GetApplicationsByVacancyAsync(int vacancyId)
        {
            return await _context.Applications
                .Where(a => a.VacancyId == vacancyId)
                .Include(a => a.User)
                .Include(a => a.Resume)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<Application?> GetApplicationWithDetailsAsync(int id)
        {
            return await _context.Applications
                .Include(a => a.User)
                .Include(a => a.Vacancy)
                    .ThenInclude(v => v.Company)
                .Include(a => a.Resume)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> UpdateStatusAsync(int id, ApplicationStatus status, string? employerComment = null)
        {
            var application = await GetByIdAsync(id);
            if (application == null) return false;

            application.Status = status;
            application.StatusUpdatedAt = DateTime.UtcNow;

            if (employerComment != null)
            {
                application.EmployerComment = employerComment;
            }

            await UpdateAsync(application);
            return true;
        }

        public async Task<bool> HasUserAppliedToVacancyAsync(string userId, int vacancyId)
        {
            return await _context.Applications
                .AnyAsync(a => a.UserId == userId && a.VacancyId == vacancyId);
        }

        public async Task<int> GetApplicationsCountByVacancyAsync(int vacancyId)
        {
            return await _context.Applications
                .CountAsync(a => a.VacancyId == vacancyId);
        }

        public async Task<int> GetApplicationsCountByUserAsync(string userId)
        {
            return await _context.Applications
                .CountAsync(a => a.UserId == userId);
        }

        public async Task<IEnumerable<Application>> GetApplicationsByCompanyAsync(int companyId)
        {
            return await _context.Applications
                .Include(a => a.Vacancy)
                .Where(a => a.Vacancy.CompanyId == companyId)
                .Include(a => a.User)
                .Include(a => a.Resume)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }
    }
}
