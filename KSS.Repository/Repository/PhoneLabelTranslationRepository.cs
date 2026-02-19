using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class PhoneLabelTranslationRepository : BaseRepository<PersonDbContext, PhoneLabelTranslation>, IPhoneLabelTranslationRepository
    {
        public PhoneLabelTranslationRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
