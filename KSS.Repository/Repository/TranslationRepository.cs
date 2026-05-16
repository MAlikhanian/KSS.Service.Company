using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class TranslationRepository : BaseRepository<CompanyDbContext, Translation>, ITranslationRepository
    {
        public TranslationRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
