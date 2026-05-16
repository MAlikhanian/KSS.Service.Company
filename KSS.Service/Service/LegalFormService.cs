using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class LegalFormService : BaseService<LegalForm, LegalFormDto, LegalFormDto, LegalFormDto>, ILegalFormService
    {
        public LegalFormService(IMapper mapper, ILegalFormRepository repository) : base(mapper, repository) { }
    }
}
