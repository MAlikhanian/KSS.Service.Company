using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class NameHistoryRepository : BaseRepository<CompanyDbContext, NameHistory>, INameHistoryRepository
    {
        public NameHistoryRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
