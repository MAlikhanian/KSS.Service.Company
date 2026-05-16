using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class FinancialInfoRepository : BaseRepository<CompanyDbContext, FinancialInfo>, IFinancialInfoRepository
    {
        public FinancialInfoRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
