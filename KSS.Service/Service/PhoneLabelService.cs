using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class PhoneLabelService : BaseService<PhoneLabel, PhoneLabelDto, PhoneLabelDto, PhoneLabelDto>, IPhoneLabelService, IReferenceData
    {
        public PhoneLabelService(IMapper mapper, IPhoneLabelRepository repository) : base(mapper, repository) { }
    }
}
