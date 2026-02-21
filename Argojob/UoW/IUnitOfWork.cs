using Agrojob.Repositories;

namespace Agrojob.UoW
{
    /// <summary>
    /// Интерфейс Unit of Work для группировки операций
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IVacancyRepository Vacancies { get; }
        ICompanyRepository Companies { get; }
        ICategoryRepository Categories { get; }
        ILocationRepository Locations { get; }
        ITagRepository Tags { get; }
        IRequirementRepository Requirements { get; }
        IOfferRepository Offers { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
