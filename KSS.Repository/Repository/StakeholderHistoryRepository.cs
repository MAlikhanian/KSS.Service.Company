using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class StakeholderHistoryRepository : BaseRepository<CompanyDbContext, StakeholderHistory>, IStakeholderHistoryRepository
    {
        public StakeholderHistoryRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
