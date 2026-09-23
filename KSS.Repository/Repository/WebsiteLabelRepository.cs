using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class WebsiteLabelRepository : BaseRepository<PersonDbContext, WebsiteLabel>, IWebsiteLabelRepository
    {
        public WebsiteLabelRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
