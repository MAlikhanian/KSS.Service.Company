using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class IndustryService : BaseService<Industry, IndustryDto, IndustryDto, IndustryDto>, IIndustryService
    {
        public IndustryService(IMapper mapper, IIndustryRepository repository) : base(mapper, repository) { }
    }
}
