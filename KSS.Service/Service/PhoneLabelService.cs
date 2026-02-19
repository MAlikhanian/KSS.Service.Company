using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class PhoneLabelService : BaseService<PhoneLabel, PhoneLabelDto, PhoneLabelDto, PhoneLabelDto>, IPhoneLabelService
    {
        public PhoneLabelService(IMapper mapper, IPhoneLabelRepository repository) : base(mapper, repository) { }
    }
}
