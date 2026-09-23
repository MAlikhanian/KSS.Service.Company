using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class CompanySoftwareRepository : BaseRepository<PersonDbContext, CompanySoftware>, ICompanySoftwareRepository
    {
        public CompanySoftwareRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
