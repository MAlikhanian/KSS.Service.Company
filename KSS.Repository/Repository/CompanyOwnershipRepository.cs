using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyOwnershipRepository : BaseRepository<CompanyDbContext, CompanyOwnership>, ICompanyOwnershipRepository
    {
        public CompanyOwnershipRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
