using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class EmailLabelTranslationService : BaseService<EmailLabelTranslation, EmailLabelTranslationDto, EmailLabelTranslationDto, EmailLabelTranslationDto>, IEmailLabelTranslationService
    {
        public EmailLabelTranslationService(IMapper mapper, IEmailLabelTranslationRepository repository) : base(mapper, repository) { }
    }
}
