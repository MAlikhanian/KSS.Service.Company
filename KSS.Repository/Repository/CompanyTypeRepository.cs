using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyTypeRepository : BaseRepository<CompanyDbContext, CompanyType>, ICompanyTypeRepository
    {
        public CompanyTypeRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
