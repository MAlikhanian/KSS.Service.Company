using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class PhoneRepository : BaseRepository<PersonDbContext, Phone>, IPhoneRepository
    {
        public PhoneRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
