using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class NameHistoryTranslationRepository : BaseRepository<CompanyDbContext, NameHistoryTranslation>, INameHistoryTranslationRepository
    {
        public NameHistoryTranslationRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
