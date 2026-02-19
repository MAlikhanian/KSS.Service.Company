using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class PhoneLabelRepository : BaseRepository<PersonDbContext, PhoneLabel>, IPhoneLabelRepository
    {
        public PhoneLabelRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
