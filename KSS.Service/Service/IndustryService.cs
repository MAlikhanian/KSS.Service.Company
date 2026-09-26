using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class IndustryService : BaseService<Industry, IndustryDto, IndustryDto, IndustryDto>, IIndustryService, IReferenceData
    {
        public IndustryService(IMapper mapper, IIndustryRepository repository) : base(mapper, repository) { }
    }
}
