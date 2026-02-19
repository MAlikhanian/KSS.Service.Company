using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class EmailService : BaseService<Email, EmailDto, EmailDto, EmailDto>, IEmailService
    {
        public EmailService(IMapper mapper, IEmailRepository repository) : base(mapper, repository) { }

        public override async Task AddAsync(Email item, bool saveChanges = true)
        {
            item.EmailAddress = EmailHelper.NormalizeEmail(item.EmailAddress);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(EmailDto item, bool saveChanges = true)
        {
            item.EmailAddress = EmailHelper.NormalizeEmail(item.EmailAddress);
            var entity = _mapper.Map<Email>(item);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(Email item, bool saveChanges = true)
        {
            item.EmailAddress = EmailHelper.NormalizeEmail(item.EmailAddress);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(EmailDto item, bool saveChanges = true)
        {
            item.EmailAddress = EmailHelper.NormalizeEmail(item.EmailAddress);
            var entity = _mapper.Map<Email>(item);
            base.Update(entity, saveChanges);
        }
    }
}
