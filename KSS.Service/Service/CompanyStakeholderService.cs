using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyStakeholderService : BaseService<CompanyStakeholder, CompanyStakeholderDto, CompanyStakeholderDto, CompanyStakeholderDto>, ICompanyStakeholderService
    {
        public CompanyStakeholderService(IMapper mapper, ICompanyStakeholderRepository repository) : base(mapper, repository) { }
    }
}
