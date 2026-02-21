using Agrojob.Data;
using Agrojob.Repositories;

namespace Agrojob.UoW
{
    /// <summary>
    /// Реализация Unit of Work
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IVacancyRepository? _vacancyRepository;
        private ICompanyRepository? _companyRepository;
        private ICategoryRepository? _categoryRepository;
        private ILocationRepository? _locationRepository;
        private ITagRepository? _tagRepository;
        private IRequirementRepository? _requirementRepository;
        private IOfferRepository? _offerRepository;

        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IVacancyRepository Vacancies =>
            _vacancyRepository ??= new VacancyRepository(_context);

        public ICompanyRepository Companies =>
            _companyRepository ??= new CompanyRepository(_context);

        public ICategoryRepository Categories =>
            _categoryRepository ??= new CategoryRepository(_context);

        public ILocationRepository Locations =>
            _locationRepository ??= new LocationRepository(_context);

        public ITagRepository Tags =>
            _tagRepository ??= new TagRepository(_context);

        public IRequirementRepository Requirements =>
            _requirementRepository ??= new RequirementRepository(_context);

        public IOfferRepository Offers =>
            _offerRepository ??= new OfferRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
