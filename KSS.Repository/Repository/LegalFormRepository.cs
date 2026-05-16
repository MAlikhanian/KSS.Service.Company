using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class LegalFormRepository : BaseRepository<CompanyDbContext, LegalForm>, ILegalFormRepository
    {
        public LegalFormRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
