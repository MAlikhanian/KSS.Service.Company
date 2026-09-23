using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyDocumentRepository : BaseRepository<CompanyDbContext, CompanyDocument>, ICompanyDocumentRepository
    {
        public CompanyDocumentRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
