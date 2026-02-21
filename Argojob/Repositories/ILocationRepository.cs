using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<IEnumerable<Location>> GetLocationsByRegionAsync(string region);
        Task<Location?> GetLocationByNameAsync(string name);
        Task<IEnumerable<string>> GetAllRegionsAsync();
        Task<int> GetVacanciesCountAsync(int locationId);
    }
}
