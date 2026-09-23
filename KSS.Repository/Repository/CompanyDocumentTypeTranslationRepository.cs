using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyDocumentTypeTranslationRepository : BaseRepository<CompanyDbContext, CompanyDocumentTypeTranslation>, ICompanyDocumentTypeTranslationRepository
    {
        public CompanyDocumentTypeTranslationRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
