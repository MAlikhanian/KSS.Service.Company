using KSS.Dto;

namespace KSS.Service.IService
{
    public interface ICompanySoftwareManagementService
    {
        /// <summary>All active categories as slots, LEFT-joined to this company's picks. Null if caller lacks Information access.</summary>
        Task<List<CompanySoftwareSlotDto>?> GetSlotsAsync(Guid companyId, Guid callerPersonId, short languageId = 12);
        Task UpsertAsync(Guid companyId, CompanySoftwareUpsertDto dto);
        Task ClearAsync(Guid companyId, byte softwareCategoryId);
        Task<List<SoftwareCatalogItemDto>> GetSoftwareCatalogAsync(short languageId = 12);
    }
}
