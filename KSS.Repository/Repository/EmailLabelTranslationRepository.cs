using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class EmailLabelTranslationRepository : BaseRepository<PersonDbContext, EmailLabelTranslation>, IEmailLabelTranslationRepository
    {
        public EmailLabelTranslationRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
