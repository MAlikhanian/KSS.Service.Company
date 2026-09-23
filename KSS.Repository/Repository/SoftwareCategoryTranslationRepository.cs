using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class SoftwareCategoryTranslationRepository : BaseRepository<PersonDbContext, SoftwareCategoryTranslation>, ISoftwareCategoryTranslationRepository
    {
        public SoftwareCategoryTranslationRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
