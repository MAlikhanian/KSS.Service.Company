using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyStakeholderHistoryService : BaseService<CompanyStakeholderHistory, CompanyStakeholderHistoryDto, CompanyStakeholderHistoryDto, CompanyStakeholderHistoryDto>, ICompanyStakeholderHistoryService
    {
        public CompanyStakeholderHistoryService(IMapper mapper, ICompanyStakeholderHistoryRepository repository) : base(mapper, repository) { }
    }
}
