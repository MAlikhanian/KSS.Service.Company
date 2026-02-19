using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyNameHistoryService : BaseService<CompanyNameHistory, CompanyNameHistoryDto, CompanyNameHistoryDto, CompanyNameHistoryDto>, ICompanyNameHistoryService
    {
        public CompanyNameHistoryService(IMapper mapper, ICompanyNameHistoryRepository repository) : base(mapper, repository) { }
    }
}
