using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyStakeholderHistoryRepository : BaseRepository<CompanyDbContext, CompanyStakeholderHistory>, ICompanyStakeholderHistoryRepository
    {
        public CompanyStakeholderHistoryRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
