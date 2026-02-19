using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyNameHistoryTranslationRepository : BaseRepository<CompanyDbContext, CompanyNameHistoryTranslation>, ICompanyNameHistoryTranslationRepository
    {
        public CompanyNameHistoryTranslationRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
