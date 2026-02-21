using Agrojob.Models;
using Agrojob.Repositories.Base;

namespace Agrojob.Repositories
{
    public interface IOfferRepository : IRepository<Offer>
    {
        Task<IEnumerable<Offer>> GetOffersByVacancyAsync(int vacancyId);
        Task<bool> DeleteAllByVacancyAsync(int vacancyId);
        Task<List<int>> AddRangeToVacancyAsync(int vacancyId, List<string> offerTexts);
    }
}
