using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper.Model;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyService : BaseService<Company, CompanyDto, CompanyInsertDto, CompanyDto>, ICompanyService
    {
        private readonly ICompanyRepository _repository;
        private readonly ICurrentCompany _currentCompany;
        private readonly ICompanyOwnershipService _ownershipService;

        public CompanyService(
            IMapper mapper,
            ICompanyRepository repository,
            ICurrentCompany currentCompany,
            ICompanyOwnershipService ownershipService) : base(mapper, repository)
        {
            _repository = repository;
            _currentCompany = currentCompany;
            _ownershipService = ownershipService;
        }

        // Tenant scoping: when an owner company is selected (X-Company-Id), narrow
        // the list to company records owned by that owner company. No header ->
        // legacy behaviour (all companies).
        public override async Task<IEnumerable<Company>> ToListAsync()
        {
            var all = await base.ToListAsync();
            if (_currentCompany.CompanyId is Guid ownerCompanyId)
            {
                var ownedIds = (await _ownershipService.ListCompanyIdsByOwnerAsync(ownerCompanyId)).ToHashSet();
                return all.Where(c => ownedIds.Contains(c.Id)).ToList();
            }
            return all;
        }

        public override async Task<Company> FindAsync(Filter id)
        {
            var company = await base.FindAsync(id);
            if (company != null && _currentCompany.CompanyId is Guid ownerCompanyId)
            {
                if (!await _ownershipService.IsOwnedAsync(ownerCompanyId, company.Id))
                    throw new KSS.Helper.BusinessRuleException("این شرکت به شرکت انتخاب‌شده تعلق ندارد.");
            }
            return company;
        }
    }
}
