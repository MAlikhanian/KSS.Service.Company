using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class AccessRepository : BaseRepository<CompanyDbContext, Access>, IAccessRepository
    {
        public AccessRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
