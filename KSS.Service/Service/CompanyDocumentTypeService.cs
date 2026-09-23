using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyDocumentTypeService : BaseService<CompanyDocumentType, CompanyDocumentTypeDto, CompanyDocumentTypeDto, CompanyDocumentTypeDto>, ICompanyDocumentTypeService
    {
        public CompanyDocumentTypeService(IMapper mapper, ICompanyDocumentTypeRepository repository) : base(mapper, repository) { }
    }
}
