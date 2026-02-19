using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class AddressLabelRepository : BaseRepository<PersonDbContext, AddressLabel>, IAddressLabelRepository
    {
        public AddressLabelRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
