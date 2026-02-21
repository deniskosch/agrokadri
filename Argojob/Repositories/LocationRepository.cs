using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        public LocationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Location>> GetLocationsByRegionAsync(string region)
        {
            return await _context.Locations
                .Where(l => l.Region != null && l.Region.ToLower().Contains(region.ToLower()))
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<Location?> GetLocationByNameAsync(string name)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(l => l.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<string>> GetAllRegionsAsync()
        {
            return await _context.Locations
                .Where(l => l.Region != null)
                .Select(l => l.Region!)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        public async Task<int> GetVacanciesCountAsync(int locationId)
        {
            return await _context.Vacancies
                .CountAsync(v => v.LocationId == locationId && v.IsActive);
        }
    }
}
