using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class LegalFormService : BaseService<LegalForm, LegalFormDto, LegalFormDto, LegalFormDto>, ILegalFormService, IReferenceData
    {
        public LegalFormService(IMapper mapper, ILegalFormRepository repository) : base(mapper, repository) { }
    }
}
