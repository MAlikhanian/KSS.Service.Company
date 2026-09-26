using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class EmailLabelService : BaseService<EmailLabel, EmailLabelDto, EmailLabelDto, EmailLabelDto>, IEmailLabelService, IReferenceData
    {
        public EmailLabelService(IMapper mapper, IEmailLabelRepository repository) : base(mapper, repository) { }
    }
}
