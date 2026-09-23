using KSS.Dto;
using KSS.Entity;

namespace KSS.Service.IService
{
    public interface ICompanyDocumentService : IBaseService<CompanyDocument, CompanyDocumentViewDto, CompanyDocumentInsertDto, CompanyDocumentUpdateDto>
    {
        /// <summary>A company's document metadata rows (newest first).</summary>
        Task<List<CompanyDocumentViewDto>> GetByCompanyAsync(Guid companyId);

        /// <summary>Create a document row and return it with the backend-stamped GUID id.</summary>
        Task<CompanyDocumentViewDto> CreateAsync(CompanyDocumentInsertDto dto);

        /// <summary>Delete a document row by key.</summary>
        void DeleteById(Guid id);
    }
}
