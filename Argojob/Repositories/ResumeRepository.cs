using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    public class ResumeRepository : Repository<Resume>, IResumeRepository
    {
        public ResumeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Resume>> GetResumesByUserAsync(string userId)
        {
            return await _context.Resumes
                .Where(r => r.UserId == userId)
                .Include(r => r.Category)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Resume?> GetResumeWithDetailsAsync(int id)
        {
            return await _context.Resumes
                .Include(r => r.User)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Resume>> SearchResumesAsync(string searchTerm, int? categoryId = null)
        {
            var query = _context.Resumes
                .Where(r => r.IsActive && r.IsPublished)
                .Include(r => r.User)
                .Include(r => r.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(searchTerm) ||
                    r.FullName.ToLower().Contains(searchTerm) ||
                    (r.Skills != null && r.Skills.ToLower().Contains(searchTerm)) ||
                    (r.About != null && r.About.ToLower().Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(r => r.CategoryId == categoryId);
            }

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ToggleStatusAsync(int id, bool isActive)
        {
            var resume = await GetByIdAsync(id);
            if (resume == null) return false;

            resume.IsActive = isActive;
            resume.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(resume);
            return true;
        }

        public async Task<int> GetUserResumesCountAsync(string userId)
        {
            return await _context.Resumes
                .CountAsync(r => r.UserId == userId);
        }
    }
}
