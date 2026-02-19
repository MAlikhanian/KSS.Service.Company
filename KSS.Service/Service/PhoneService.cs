using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class PhoneService : BaseService<Phone, PhoneDto, PhoneDto, PhoneDto>, IPhoneService
    {
        public PhoneService(IMapper mapper, IPhoneRepository repository) : base(mapper, repository) { }

        public override async Task AddAsync(Phone item, bool saveChanges = true)
        {
            PhoneHelper.ValidateE164(item.PhoneNumber);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(PhoneDto item, bool saveChanges = true)
        {
            PhoneHelper.ValidateE164(item.PhoneNumber);
            var entity = _mapper.Map<Phone>(item);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(Phone item, bool saveChanges = true)
        {
            PhoneHelper.ValidateE164(item.PhoneNumber);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(PhoneDto item, bool saveChanges = true)
        {
            PhoneHelper.ValidateE164(item.PhoneNumber);
            var entity = _mapper.Map<Phone>(item);
            base.Update(entity, saveChanges);
        }
    }
}
