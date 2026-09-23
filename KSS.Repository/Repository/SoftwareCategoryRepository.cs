using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class SoftwareCategoryRepository : BaseRepository<PersonDbContext, SoftwareCategory>, ISoftwareCategoryRepository
    {
        public SoftwareCategoryRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
