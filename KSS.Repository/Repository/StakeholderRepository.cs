using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class StakeholderRepository : BaseRepository<CompanyDbContext, Stakeholder>, IStakeholderRepository
    {
        public StakeholderRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
