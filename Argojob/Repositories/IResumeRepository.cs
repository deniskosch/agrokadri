using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface IResumeRepository : IRepository<Resume>
    {
        Task<IEnumerable<Resume>> GetResumesByUserAsync(string userId);
        Task<Resume?> GetResumeWithDetailsAsync(int id);
        Task<IEnumerable<Resume>> SearchResumesAsync(string searchTerm, int? categoryId = null);
        Task<bool> ToggleStatusAsync(int id, bool isActive);
        Task<int> GetUserResumesCountAsync(string userId);
    }
}
