using KSS.Entity;
using KSS.Repository.IRepository;
using CompanyDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanyDocumentTypeRepository : BaseRepository<CompanyDbContext, CompanyDocumentType>, ICompanyDocumentTypeRepository
    {
        public CompanyDocumentTypeRepository(CompanyDbContext dbContext) : base(dbContext) { }
    }
}
