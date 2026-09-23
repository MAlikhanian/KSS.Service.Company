using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyOwnershipService
        : BaseService<CompanyOwnership, CompanyOwnershipViewDto, CompanyOwnershipInsertDto, CompanyOwnershipViewDto>, ICompanyOwnershipService
    {
        private readonly ICompanyOwnershipRepository _repository;

        public CompanyOwnershipService(IMapper mapper, ICompanyOwnershipRepository repository) : base(mapper, repository)
        {
            _repository = repository;
        }

        public async Task<CompanyOwnershipViewDto> AssignAsync(CompanyOwnershipInsertDto dto, Guid callerPersonId)
        {
            if (dto.OwnerCompanyId == Guid.Empty || dto.CompanyId == Guid.Empty)
                throw new BusinessRuleException("شناسهٔ شرکت مالک و شرکت الزامی است.");
            if (dto.OwnerCompanyId == dto.CompanyId)
                throw new BusinessRuleException("یک شرکت نمی‌تواند مالک خودش باشد.");

            var existing = await _repository.SingleOrDefaultAsync(
                o => o.OwnerCompanyId == dto.OwnerCompanyId && o.CompanyId == dto.CompanyId);
            if (existing != null)
                return _mapper.Map<CompanyOwnershipViewDto>(existing);

            var entity = new CompanyOwnership
            {
                OwnerCompanyId = dto.OwnerCompanyId,
                CompanyId = dto.CompanyId,
                CreatedBy = callerPersonId,
            };
            await _repository.AddAsync(entity);
            return _mapper.Map<CompanyOwnershipViewDto>(entity);
        }

        public async Task UnassignAsync(Guid ownerCompanyId, Guid companyId)
        {
            var existing = await _repository.SingleOrDefaultAsync(
                o => o.OwnerCompanyId == ownerCompanyId && o.CompanyId == companyId);
            if (existing == null) return;
            _repository.Remove(existing);
        }

        public async Task<List<Guid>> ListCompanyIdsByOwnerAsync(Guid ownerCompanyId)
        {
            var rows = await _repository.ToListAsync(o => o.OwnerCompanyId == ownerCompanyId && o.IsActive);
            return rows.Select(o => o.CompanyId).Distinct().ToList();
        }

        public async Task<List<Guid>> ListOwnerIdsByCompanyAsync(Guid companyId)
        {
            var rows = await _repository.ToListAsync(o => o.CompanyId == companyId && o.IsActive);
            return rows.Select(o => o.OwnerCompanyId).Distinct().ToList();
        }

        public async Task<bool> IsOwnedAsync(Guid ownerCompanyId, Guid companyId)
        {
            var count = await _repository.CountAsync(
                o => o.OwnerCompanyId == ownerCompanyId && o.CompanyId == companyId && o.IsActive);
            return count > 0;
        }
    }
}
