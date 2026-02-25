using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyFinancialInfoRepository : BaseRepository<CompanyDbContext, CompanyFinancialInfo>, ICompanyFinancialInfoRepository
    {
        public CompanyFinancialInfoRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
