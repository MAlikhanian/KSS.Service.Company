using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class AddressTranslationRepository : BaseRepository<PersonDbContext, AddressTranslation>, IAddressTranslationRepository
    {
        public AddressTranslationRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
