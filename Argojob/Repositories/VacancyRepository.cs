using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    /// <summary>
    /// Репозиторий для работы с вакансиями
    /// </summary>
    public class VacancyRepository : Repository<Vacancy>, IVacancyRepository
    {
        public VacancyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Vacancy?> GetVacancyWithDetailsAsync(int id)
        {
            return await _context.Vacancies
                .Include(v => v.Company)
                .Include(v => v.Location)
                .Include(v => v.Category)
                .Include(v => v.CreatedBy)
                .Include(v => v.VacancyTags)
                    .ThenInclude(vt => vt.Tag)
                .Include(v => v.Requirements)
                .Include(v => v.Offers)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Vacancy>> GetActiveVacanciesAsync()
        {
            return await _context.Vacancies
                .Where(v => v.IsActive)
                .Include(v => v.Company)
                .Include(v => v.Location)
                .Include(v => v.Category)
                .OrderByDescending(v => v.PostedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacancy>> GetSeasonalVacanciesAsync()
        {
            return await _context.Vacancies
                .Where(v => v.IsSeasonal && v.IsActive)
                .Include(v => v.Company)
                .Include(v => v.Location)
                .Include(v => v.Category)
                .OrderByDescending(v => v.PostedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacancy>> GetVacanciesByCategoryAsync(int categoryId)
        {
            return await _context.Vacancies
                .Where(v => v.CategoryId == categoryId && v.IsActive)
                .Include(v => v.Company)
                .Include(v => v.Location)
                .OrderByDescending(v => v.PostedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacancy>> GetVacanciesByCompanyAsync(int companyId)
        {
            return await _context.Vacancies
                .Where(v => v.CompanyId == companyId && v.IsActive)
                .Include(v => v.Location)
                .Include(v => v.Category)
                .OrderByDescending(v => v.PostedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacancy>> GetVacanciesByLocationAsync(int locationId)
        {
            return await _context.Vacancies
                .Where(v => v.LocationId == locationId && v.IsActive)
                .Include(v => v.Company)
                .Include(v => v.Category)
                .OrderByDescending(v => v.PostedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacancy>> SearchVacanciesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveVacanciesAsync();

            searchTerm = searchTerm.ToLower();

            return await _context.Vacancies
                .Where(v => v.IsActive && (
                    v.Title.ToLower().Contains(searchTerm) ||
                    v.Description.ToLower().Contains(searchTerm) ||
                    v.Company.Name.ToLower().Contains(searchTerm) ||
                    v.Category.Name.ToLower().Contains(searchTerm)
                ))
                .Include(v => v.Company)
                .Include(v => v.Location)
                .Include(v => v.Category)
                .OrderByDescending(v => v.PostedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacancy>> GetLatestVacanciesAsync(int count)
        {
            return await _context.Vacancies
                .Where(v => v.IsActive)
                .Include(v => v.Company)
                .Include(v => v.Location)
                .Include(v => v.Category)
                .OrderByDescending(v => v.PostedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetVacancyStatsAsync()
        {
            var stats = new Dictionary<string, int>();

            stats["Total"] = await _context.Vacancies.CountAsync();
            stats["Active"] = await _context.Vacancies.CountAsync(v => v.IsActive);
            stats["Seasonal"] = await _context.Vacancies.CountAsync(v => v.IsSeasonal);
            //stats["Urgent"] = await _context.Vacancies.CountAsync(v => v.IsUrgent);

            return stats;
        }

        public async Task<bool> PublishVacancyAsync(int id)
        {
            var vacancy = await GetByIdAsync(id);
            if (vacancy == null)
                return false;

            vacancy.IsActive = true;
            //vacancy.PublishedAt = DateTime.UtcNow;

            return await UpdateAsync(vacancy);
        }

        public async Task<bool> ArchiveVacancyAsync(int id)
        {
            var vacancy = await GetByIdAsync(id);
            if (vacancy == null)
                return false;

            vacancy.IsActive = false;

            return await UpdateAsync(vacancy);
        }

        public async Task<int> IncrementViewsAsync(int id)
        {
            var vacancy = await GetByIdAsync(id);
            if (vacancy == null)
                return 0;

            vacancy.ViewsCount++;
            await UpdateAsync(vacancy);

            return vacancy.ViewsCount;
        }

        public async Task<IEnumerable<Vacancy>> FilterVacanciesAsync(
            int? categoryId = null,
            int? locationId = null,
            int? companyId = null,
            bool? isSeasonal = null,
            decimal? minSalary = null,
            decimal? maxSalary = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Vacancies
                .Where(v => v.IsActive)
                .Include(v => v.Company)
                .Include(v => v.Location)
                .Include(v => v.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(v => v.CategoryId == categoryId);

            if (locationId.HasValue)
                query = query.Where(v => v.LocationId == locationId);

            if (companyId.HasValue)
                query = query.Where(v => v.CompanyId == companyId);

            if (isSeasonal.HasValue)
                query = query.Where(v => v.IsSeasonal == isSeasonal);

            //if (minSalary.HasValue)
            //    query = query.Where(v => v.SalaryMin >= minSalary || v.IsSalaryNegotiable);

            //if (maxSalary.HasValue)
            //    query = query.Where(v => v.SalaryMax <= maxSalary || v.IsSalaryNegotiable);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(v =>
                    v.Title.ToLower().Contains(searchTerm) ||
                    v.Description.ToLower().Contains(searchTerm) ||
                    v.Company.Name.ToLower().Contains(searchTerm));
            }

            // Пагинация
            query = query
                .OrderByDescending(v => v.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return await query.ToListAsync();
        }
    }
}
