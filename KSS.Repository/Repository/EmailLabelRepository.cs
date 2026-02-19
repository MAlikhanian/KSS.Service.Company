using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class EmailLabelRepository : BaseRepository<PersonDbContext, EmailLabel>, IEmailLabelRepository
    {
        public EmailLabelRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
