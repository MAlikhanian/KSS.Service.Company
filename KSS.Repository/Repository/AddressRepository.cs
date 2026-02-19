using KSS.Entity;
using KSS.Repository.IRepository;
using PersonDbContext = KSS.Data.DbContexts.MainDbContext;

namespace KSS.Repository.Repository
{
    public class AddressRepository : BaseRepository<PersonDbContext, Address>, IAddressRepository
    {
        public AddressRepository(PersonDbContext dbContext) : base(dbContext) { }
    }
}
