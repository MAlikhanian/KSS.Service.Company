using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class StakeholderTypeRepository : BaseRepository<CompanyDbContext, StakeholderType>, IStakeholderTypeRepository
    {
        public StakeholderTypeRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
