using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyStakeholderRepository : BaseRepository<CompanyDbContext, CompanyStakeholder>, ICompanyStakeholderRepository
    {
        public CompanyStakeholderRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
