using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyTranslationRepository : BaseRepository<CompanyDbContext, CompanyTranslation>, ICompanyTranslationRepository
    {
        public CompanyTranslationRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
