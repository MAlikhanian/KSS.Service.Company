using AutoMapper;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class SoftwareService : BaseService<Software, SoftwareDto, SoftwareInsertDto, SoftwareUpdateDto>, ISoftwareService
    {
        private readonly MainDbContext _db;

        public SoftwareService(IMapper mapper, ISoftwareRepository repository, MainDbContext db) : base(mapper, repository)
        {
            _db = db;
        }

        // Base UpdateDto maps the DTO onto a brand-new disconnected Software and marks
        // the WHOLE entity Modified, which wipes CreatedAt/CreatedBy (they're not on
        // SoftwareUpdateDto, so they map to default values). Load the tracked row and
        // patch only the editable fields so CreatedAt/CreatedBy are preserved and
        // ApplyEntityDefaults() stamps UpdatedAt/UpdatedBy correctly.
        public override void UpdateDto(SoftwareUpdateDto item, bool saveChanges = true)
        {
            var entity = _db.Softwares.Find(item.Id)
                ?? throw new KeyNotFoundException($"Software {item.Id} not found");

            entity.Name = item.Name;
            entity.IsActive = item.IsActive;
            entity.CompanyId = item.CompanyId;

            if (saveChanges) _db.SaveChanges();
        }
    }
}
