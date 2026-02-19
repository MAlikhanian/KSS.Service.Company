using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyNameHistoryRepository : BaseRepository<CompanyDbContext, CompanyNameHistory>, ICompanyNameHistoryRepository
    {
        public CompanyNameHistoryRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
