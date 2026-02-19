using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyRepository : BaseRepository<CompanyDbContext, Company>, ICompanyRepository
    {
        public CompanyRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
