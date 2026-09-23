using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyDocumentService : BaseService<CompanyDocument, CompanyDocumentViewDto, CompanyDocumentInsertDto, CompanyDocumentUpdateDto>, ICompanyDocumentService
    {
        private readonly ICompanyDocumentRepository _companyDocumentRepository;

        public CompanyDocumentService(IMapper mapper, ICompanyDocumentRepository repository) : base(mapper, repository)
        {
            _companyDocumentRepository = repository;
        }

        // A company's document metadata rows (newest first).
        public async Task<List<CompanyDocumentViewDto>> GetByCompanyAsync(Guid companyId)
        {
            var rows = await _companyDocumentRepository.ToListAsync(x => x.CompanyId == companyId);
            return _mapper.Map<List<CompanyDocumentViewDto>>(rows.OrderByDescending(x => x.CreatedAt));
        }

        // Create from an InsertDto and return the created row (Id/CreatedAt are
        // stamped by MainDbContext.ApplyEntityDefaults on SaveChanges, so the
        // mapped view carries the new GUID id back to the caller).
        public async Task<CompanyDocumentViewDto> CreateAsync(CompanyDocumentInsertDto dto)
        {
            var entity = _mapper.Map<CompanyDocument>(dto);
            await AddAsync(entity, true);
            return _mapper.Map<CompanyDocumentViewDto>(entity);
        }

        // Delete by key — no full-entity bind (whose non-nullable nav props would
        // otherwise be implicitly required under [ApiController] + nullable refs).
        public void DeleteById(Guid id)
        {
            var existing = _companyDocumentRepository.Find(id);
            if (existing != null)
                Remove(existing);
        }

        /// <summary>
        /// Load the existing row first, then patch only the editable fields
        /// (document type + file name). Preserves CompanyId, StorageInstanceId,
        /// FileSize, ContentType and the audit columns — mirrors FinancialInfoService.
        /// </summary>
        public override void UpdateDto(CompanyDocumentUpdateDto item, bool saveChanges = true)
        {
            var existing = _companyDocumentRepository.Find(item.Id)
                ?? throw new KeyNotFoundException($"CompanyDocument with Id '{item.Id}' not found.");

            existing.CompanyDocumentTypeId = item.CompanyDocumentTypeId;
            // FileName is refreshed only when the BFF sends a non-empty value
            // (i.e. a new file was uploaded); keep the current name otherwise.
            if (!string.IsNullOrWhiteSpace(item.FileName))
                existing.FileName = item.FileName;

            base.Update(existing, saveChanges);
        }
    }
}
