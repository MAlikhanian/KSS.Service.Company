using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class SoftwareCategoryService : BaseService<SoftwareCategory, SoftwareCategoryDto, SoftwareCategoryDto, SoftwareCategoryDto>, ISoftwareCategoryService
    {
        public SoftwareCategoryService(IMapper mapper, ISoftwareCategoryRepository repository) : base(mapper, repository) { }
    }
}
