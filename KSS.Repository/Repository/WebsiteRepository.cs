using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class WebsiteRepository : BaseRepository<PersonDbContext, Website>, IWebsiteRepository
    {
        public WebsiteRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
