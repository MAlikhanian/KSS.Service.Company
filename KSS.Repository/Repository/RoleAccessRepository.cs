using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class RoleAccessRepository : BaseRepository<CompanyDbContext, RoleAccess>, IRoleAccessRepository
    {
        public RoleAccessRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
