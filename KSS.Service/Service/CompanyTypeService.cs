using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyTypeService : BaseService<CompanyType, CompanyTypeDto, CompanyTypeDto, CompanyTypeDto>, ICompanyTypeService
    {
        public CompanyTypeService(IMapper mapper, ICompanyTypeRepository repository) : base(mapper, repository) { }
    }
}
