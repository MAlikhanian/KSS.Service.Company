using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class EmailRepository : BaseRepository<PersonDbContext, Email>, IEmailRepository
    {
        public EmailRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
