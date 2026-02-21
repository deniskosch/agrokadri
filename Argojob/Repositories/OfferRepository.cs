using Agrojob.Data;
using Agrojob.Models;
using Agrojob.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Agrojob.Repositories
{
    public class OfferRepository : Repository<Offer>, IOfferRepository
    {
        public OfferRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Offer>> GetOffersByVacancyAsync(int vacancyId)
        {
            return await _context.Offers
                .Where(o => o.VacancyId == vacancyId)
                .OrderBy(o => o.Id)
                .ToListAsync();
        }

        public async Task<bool> DeleteAllByVacancyAsync(int vacancyId)
        {
            var offers = await GetOffersByVacancyAsync(vacancyId);
            if (!offers.Any())
                return false;

            _context.Offers.RemoveRange(offers);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<int>> AddRangeToVacancyAsync(int vacancyId, List<string> offerTexts)
        {
            var offerIds = new List<int>();

            foreach (var text in offerTexts.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var offer = new Offer
                {
                    VacancyId = vacancyId,
                    Text = text.Trim()
                };

                var id = await AddAsync(offer);
                offerIds.Add(id);
            }

            return offerIds;
        }
    }
}
