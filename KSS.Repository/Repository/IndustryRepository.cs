using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class IndustryRepository : BaseRepository<CompanyDbContext, Industry>, IIndustryRepository
    {
        public IndustryRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
