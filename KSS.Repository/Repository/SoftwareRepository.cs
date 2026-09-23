using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class SoftwareRepository : BaseRepository<PersonDbContext, Software>, ISoftwareRepository
    {
        public SoftwareRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
