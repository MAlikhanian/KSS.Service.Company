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
            ValidateEmail(item);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(EmailDto item, bool saveChanges = true)
        {
            item.EmailAddress = EmailHelper.NormalizeEmail(item.EmailAddress);
            var entity = _mapper.Map<Email>(item);
            ValidateEmail(entity);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(Email item, bool saveChanges = true)
        {
            item.EmailAddress = EmailHelper.NormalizeEmail(item.EmailAddress);
            ValidateEmail(item);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(EmailDto item, bool saveChanges = true)
        {
            item.EmailAddress = EmailHelper.NormalizeEmail(item.EmailAddress);
            var entity = _mapper.Map<Email>(item);
            ValidateEmail(entity);
            base.Update(entity, saveChanges);
        }

        private static void ValidateEmail(Email email)
        {
            // Validate VerifiedAt: if IsVerified is true, VerifiedAt must be set
            if (email.IsVerified && !email.VerifiedAt.HasValue)
            {
                throw new ArgumentException("VerifiedAt must be set when IsVerified is true.", nameof(email));
            }
        }
    }
}
