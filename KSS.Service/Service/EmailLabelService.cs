using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class EmailLabelService : BaseService<EmailLabel, EmailLabelDto, EmailLabelDto, EmailLabelDto>, IEmailLabelService
    {
        public EmailLabelService(IMapper mapper, IEmailLabelRepository repository) : base(mapper, repository) { }
    }
}
