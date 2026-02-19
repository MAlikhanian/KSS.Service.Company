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
            ValidatePhone(item);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(PhoneDto item, bool saveChanges = true)
        {
            PhoneHelper.ValidateE164(item.PhoneNumber);
            var entity = _mapper.Map<Phone>(item);
            ValidatePhone(entity);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(Phone item, bool saveChanges = true)
        {
            PhoneHelper.ValidateE164(item.PhoneNumber);
            ValidatePhone(item);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(PhoneDto item, bool saveChanges = true)
        {
            PhoneHelper.ValidateE164(item.PhoneNumber);
            var entity = _mapper.Map<Phone>(item);
            ValidatePhone(entity);
            base.Update(entity, saveChanges);
        }

        private static void ValidatePhone(Phone phone)
        {
            // Validate VerifiedAt: if IsVerified is true, VerifiedAt must be set
            if (phone.IsVerified && !phone.VerifiedAt.HasValue)
            {
                throw new ArgumentException("VerifiedAt must be set when IsVerified is true.", nameof(phone));
            }
        }
    }
}
