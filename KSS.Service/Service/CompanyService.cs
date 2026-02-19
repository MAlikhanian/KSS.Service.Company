using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyService : BaseService<Company, CompanyDto, CompanyDto, CompanyDto>, ICompanyService
    {
        public CompanyService(IMapper mapper, ICompanyRepository repository) : base(mapper, repository) { }
    }
}
