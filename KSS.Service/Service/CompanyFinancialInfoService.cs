using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyFinancialInfoService : BaseService<CompanyFinancialInfo, CompanyFinancialInfoDto, CompanyFinancialInfoDto, CompanyFinancialInfoDto>, ICompanyFinancialInfoService
    {
        private readonly ICompanyFinancialInfoRepository _financialInfoRepository;

        public CompanyFinancialInfoService(IMapper mapper, ICompanyFinancialInfoRepository repository) : base(mapper, repository)
        {
            _financialInfoRepository = repository;
        }

        public override async Task AddDtoAsync(CompanyFinancialInfoDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<CompanyFinancialInfo>(item);
            ValidateFinancialInfo(entity);
            await base.AddAsync(entity, saveChanges);
        }

        /// <summary>
        /// Load existing entity first, then only update the editable fields.
        /// This avoids DbUpdateConcurrencyException caused by AutoMapper setting
        /// CreatedAt/UpdatedAt to default values on a detached entity.
        /// </summary>
        public override void UpdateDto(CompanyFinancialInfoDto item, bool saveChanges = true)
        {
            var existing = _financialInfoRepository.Find(item.Id)
                ?? throw new KeyNotFoundException($"CompanyFinancialInfo with Id '{item.Id}' not found.");

            // Only update the editable fields — preserve CreatedAt, UpdatedAt (managed by trigger)
            existing.CompanyId = item.CompanyId;
            existing.FiscalYear = item.FiscalYear;
            existing.RegisteredCapital = item.RegisteredCapital;
            existing.NumberOfShares = item.NumberOfShares;

            ValidateFinancialInfo(existing);
            base.Update(existing, saveChanges);
        }

        private static void ValidateFinancialInfo(CompanyFinancialInfo info)
        {
            if (info.FiscalYear < 1300 || info.FiscalYear > 1500)
            {
                throw new ArgumentException("FiscalYear must be a valid Shamsi year (1300-1500).", nameof(info));
            }

            if (info.RegisteredCapital <= 0)
            {
                throw new ArgumentException("RegisteredCapital is required and must be greater than zero.", nameof(info));
            }

            if (info.NumberOfShares <= 0)
            {
                throw new ArgumentException("NumberOfShares is required and must be greater than zero.", nameof(info));
            }
        }
    }
}
