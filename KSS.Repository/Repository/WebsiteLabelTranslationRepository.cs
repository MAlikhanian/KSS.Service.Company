using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class WebsiteLabelTranslationRepository : BaseRepository<PersonDbContext, WebsiteLabelTranslation>, IWebsiteLabelTranslationRepository
    {
        public WebsiteLabelTranslationRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
