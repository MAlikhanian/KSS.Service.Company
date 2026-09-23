using KSS.Dto;
using KSS.Entity;

namespace KSS.Service.IService
{
    public interface ICompanyOwnershipService : IBaseService<CompanyOwnership, CompanyOwnershipViewDto, CompanyOwnershipInsertDto, CompanyOwnershipViewDto>
    {
        /// <summary>Assign a company record to an owner company (idempotent).</summary>
        Task<CompanyOwnershipViewDto> AssignAsync(CompanyOwnershipInsertDto dto, Guid callerPersonId);

        /// <summary>Remove a company record from an owner company. No-op if not assigned.</summary>
        Task UnassignAsync(Guid ownerCompanyId, Guid companyId);

        /// <summary>The company ids owned by an owner company.</summary>
        Task<List<Guid>> ListCompanyIdsByOwnerAsync(Guid ownerCompanyId);

        /// <summary>The owner-company ids that own a company.</summary>
        Task<List<Guid>> ListOwnerIdsByCompanyAsync(Guid companyId);

        /// <summary>True if the company is owned by the owner company.</summary>
        Task<bool> IsOwnedAsync(Guid ownerCompanyId, Guid companyId);
    }
}
