using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class AddressLabelTranslationRepository : BaseRepository<PersonDbContext, AddressLabelTranslation>, IAddressLabelTranslationRepository
    {
        public AddressLabelTranslationRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
